using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dtos.Simulation;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Services.Evaluators
{
    /// <summary>
    /// Serviço de auditoria conversacional que analisa se a resposta do chatbot possui contradições ou alucinações (fatos inventados) contra as fatias de conhecimento.
    /// </summary>
    public class FaithfulnessEvaluator : IFaithfulnessEvaluator
    {
        private readonly ILlmJudgeClient _llmJudgeClient;
        private readonly ILogger<FaithfulnessEvaluator> _logger;

        public FaithfulnessEvaluator(ILlmJudgeClient llmJudgeClient, ILogger<FaithfulnessEvaluator> logger)
        {
            _llmJudgeClient = llmJudgeClient ?? throw new ArgumentNullException(nameof(llmJudgeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Compara a resposta conversacional do chatbot com as fatias obtidas do ChromaDB e retorna um DTO estruturado de auditoria.
        /// </summary>
        public async Task<FaithfulnessResultDto> EvaluateFaithfulnessAsync(string prompt, string response, List<RagContextChunkDto> contextChunks)
        {
            var result = new FaithfulnessResultDto
            {
                Score = 0.0,
                IsFaithful = false,
                RawLlmResponse = string.Empty
            };

            try
            {
                // 1. Construir o escopo textual das fatias de referência
                var contextBuilder = new StringBuilder();
                if (contextChunks == null || contextChunks.Count == 0)
                {
                    contextBuilder.AppendLine("Nenhum contexto de RAG foi fornecido (Falta de suporte documental absoluto).");
                }
                else
                {
                    int index = 1;
                    foreach (var chunk in contextChunks)
                    {
                        contextBuilder.AppendLine($"[Fatia {index}]");
                        contextBuilder.AppendLine($"Id: {chunk.ChunkId}");
                        contextBuilder.AppendLine($"Origem: {chunk.Source}");
                        contextBuilder.AppendLine($"Conteúdo: {chunk.Content}");
                        contextBuilder.AppendLine($"Métrica Cosseno: {chunk.Distance}");
                        contextBuilder.AppendLine();
                        index++;
                    }
                }

                // 2. Estabelecer o Prompt de Sistema com diretrizes para o LLM Judge
                var systemPrompt = 
                    "Você é um auditor de qualidade e engenheiro de testes especialista em inteligência artificial conversacional.\n" +
                    "Sua missão é classificar a FIDELIDADE (Faithfulness) de uma resposta de chatbot contra as fatias de contexto RAG de referência.\n" +
                    "O objetivo é identificar ALUCINAÇÕES (fatos gerados pelo chatbot que não possuem nenhuma base ou suporte nos documentos de referência).\n\n" +
                    "Diretrizes:\n" +
                    "- Identifique cada afirmação factual na Resposta do Chatbot.\n" +
                    "- Verifique se essa afirmação pode ser diretamente deduzida ou se está explicitada nas Fatias de Referência.\n" +
                    "- Atribua um score entre 0.0 e 1.0:\n" +
                    "  * 1.0: Todas as afirmações factuais na resposta são 100% suportadas pelas referências.\n" +
                    "  * 0.0: A resposta é uma alucinação completa ou contradiz totalmente o contexto.\n" +
                    "- Você DEVE retornar estritamente um JSON estruturado com o seguinte layout:\n" +
                    "{\n" +
                    "  \"score\": 0.90,\n" +
                    "  \"reasoning\": [\n" +
                    "    \"A afirmação X é confirmada pela Fatia 1.\",\n" +
                    "    \"A afirmação Y não possui amparo nas referências, reduzindo a nota original.\"\n" +
                    "  ]\n" +
                    "}\n" +
                    "Não inclua nenhum texto de abertura ou fechamento, responda apenas com o JSON puro.";

                // 3. Estabelecer o Prompt do Usuário
                var userPromptBuilder = new StringBuilder();
                userPromptBuilder.AppendLine("=== FATIAS DE REFERÊNCIA DE RAG ===");
                userPromptBuilder.AppendLine(contextBuilder.ToString());
                userPromptBuilder.AppendLine("=== PERGUNTA DO USUÁRIO ===");
                userPromptBuilder.AppendLine(prompt);
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("=== RESPOSTA GERADA PELO CHATBOT ===");
                userPromptBuilder.AppendLine(response);
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("Avalie a fidelidade da resposta gerada, gerando o JSON:");

                // 4. Despachar para a API do LLM Judge
                var rawLlmResponse = await _llmJudgeClient.ExecuteEvaluationAsync(systemPrompt, userPromptBuilder.ToString());
                if (string.IsNullOrWhiteSpace(rawLlmResponse))
                {
                    _logger.LogWarning("O LLM Judge retornou corpo em branco. Classificando com score de alucinação.");
                    result.Reasoning.Add("A IA juíza falhou em emitir um parecer.");
                    return result;
                }

                result.RawLlmResponse = rawLlmResponse;

                // 5. Parsing resiliente do corpo JSON retornado
                var parsedResult = ParseLlmResponse(rawLlmResponse);
                if (parsedResult != null)
                {
                    result.Score = parsedResult.Score;
                    result.Reasoning = parsedResult.Reasoning;
                    result.IsFaithful = result.Score >= 0.8; // BOAS PRÁTICAS: 80% de acurácia factual mínima para fidelidade
                }
                else
                {
                    result.Score = 0.5;
                    result.Reasoning.Add("JSON mal-formatado retornado pelo LLM Judge. Resposta de fallback aplicada.");
                    result.Reasoning.Add($"Resposta crua: {rawLlmResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na execução da avaliação de alucinação Faithfulness.");
                result.Reasoning.Add($"Erro interno do avaliador: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Limpa marcas de markdown e extrai o bloco de chaves para um parsing à prova de falhas.
        /// </summary>
        private FaithfulnessResponseJson? ParseLlmResponse(string rawResponse)
        {
            try
            {
                var cleaned = rawResponse.Trim();
                if (cleaned.StartsWith("```"))
                {
                    var lines = cleaned.Split('\n');
                    var jsonBuilder = new StringBuilder();
                    foreach (var line in lines)
                    {
                        if (!line.Trim().StartsWith("```"))
                        {
                            jsonBuilder.AppendLine(line);
                        }
                    }
                    cleaned = jsonBuilder.ToString().Trim();
                }

                int firstBrace = cleaned.IndexOf('{');
                int lastBrace = cleaned.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<FaithfulnessResponseJson>(cleaned, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao analisar JSON retornado pelo LLM Judge: {Raw}", rawResponse);
                return null;
            }
        }

        private class FaithfulnessResponseJson
        {
            public double Score { get; set; }
            public List<string> Reasoning { get; set; } = new List<string>();
        }
    }
}
