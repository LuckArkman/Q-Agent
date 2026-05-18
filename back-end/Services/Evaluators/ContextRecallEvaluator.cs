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
    /// Serviço de auditoria conversacional que analisa se o chatbot citou e cobriu todas as informações cruciais e diretrizes de negócios contidas nas fatias RAG de referência.
    /// </summary>
    public class ContextRecallEvaluator : IContextRecallEvaluator
    {
        private readonly ILlmJudgeClient _llmJudgeClient;
        private readonly ILogger<ContextRecallEvaluator> _logger;

        public ContextRecallEvaluator(ILlmJudgeClient llmJudgeClient, ILogger<ContextRecallEvaluator> logger)
        {
            _llmJudgeClient = llmJudgeClient ?? throw new ArgumentNullException(nameof(llmJudgeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Compara a resposta gerada com as fatias RAG de referência e calcula o nível de cobertura e completude (Context Recall).
        /// </summary>
        public async Task<ContextRecallResultDto> EvaluateContextRecallAsync(string prompt, string response, List<RagContextChunkDto> contextChunks)
        {
            var result = new ContextRecallResultDto
            {
                Score = 0.0,
                IsRecalled = false,
                RawLlmResponse = string.Empty
            };

            try
            {
                // 1. Construir o escopo textual das fatias RAG
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
                        contextBuilder.AppendLine($"[Fatia RAG {index}]");
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
                    "Você é um auditor de qualidade e engenheiro de testes especializado em arquiteturas RAG (Retrieval-Augmented Generation).\n" +
                    "Sua missão é classificar o RECALL DE CONTEXTO (Context Recall) de uma resposta de chatbot contra as fatias de contexto RAG de referência.\n" +
                    "O objetivo é avaliar se o Chatbot cobriu TODOS OS FATOS CRÚCIAIS, instruções e regras de negócios descritos nas fatias de referência para responder adequadamente à dúvida do usuário.\n\n" +
                    "Diretrizes:\n" +
                    "- Identifique cada regra de negócio ou fato importante presente nas Fatias de Referência que seja relevante para a Pergunta.\n" +
                    "- Verifique se cada um desses pontos críticos foi mencionado ou refletido na Resposta do Chatbot.\n" +
                    "- Não aceite sinonímias fracas que descaracterizem ou diluam regras de negócios estritas indexadas na base.\n" +
                    "- O score deve variar de 0.0 a 1.0 (razão de pontos críticos cobertos versus total de pontos críticos presentes no contexto).\n" +
                    "- Você DEVE retornar estritamente um JSON estruturado com o seguinte layout:\n" +
                    "{\n" +
                    "  \"score\": 0.90,\n" +
                    "  \"reasoning\": [\n" +
                    "    \"O fato A presente na Fatia 1 foi perfeitamente coberto pelo chatbot.\",\n" +
                    "    \"A regra estrita de negócios B presente na Fatia 2 foi totalmente omitida, reduzindo o score.\"\n" +
                    "  ]\n" +
                    "}\n" +
                    "Não inclua nenhum texto de abertura ou fechamento, responda apenas com o JSON puro.";

                // 3. Estabelecer o Prompt do Usuário
                var userPromptBuilder = new StringBuilder();
                userPromptBuilder.AppendLine("=== PERGUNTA DO USUÁRIO ===");
                userPromptBuilder.AppendLine(prompt);
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("=== FATIAS DE REFERÊNCIA DE RAG ===");
                userPromptBuilder.AppendLine(contextBuilder.ToString());
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("=== RESPOSTA GERADA PELO CHATBOT ===");
                userPromptBuilder.AppendLine(response);
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("Avalie o recall de contexto, gerando o JSON:");

                // 4. Despachar para a API do LLM Judge
                var rawLlmResponse = await _llmJudgeClient.ExecuteEvaluationAsync(systemPrompt, userPromptBuilder.ToString());
                if (string.IsNullOrWhiteSpace(rawLlmResponse))
                {
                    _logger.LogWarning("O LLM Judge retornou corpo em branco para recall de contexto. Atribuindo nota mínima.");
                    result.Reasoning.Add("A IA juíza falhou em responder.");
                    return result;
                }

                result.RawLlmResponse = rawLlmResponse;

                // 5. Parsing do JSON de forma resiliente
                var parsedResult = ParseLlmResponse(rawLlmResponse);
                if (parsedResult != null)
                {
                    result.Score = parsedResult.Score;
                    result.Reasoning = parsedResult.Reasoning;
                    result.IsRecalled = result.Score >= 0.8; // BOAS PRÁTICAS: 80% como score mínimo de completude factual aprovado
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
                _logger.LogError(ex, "Erro na execução da avaliação de recall de contexto.");
                result.Reasoning.Add($"Erro interno do avaliador: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Limpa o corpo textual do JSON para extração à prova de falhas.
        /// </summary>
        private ContextRecallResponseJson? ParseLlmResponse(string rawResponse)
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

                return JsonSerializer.Deserialize<ContextRecallResponseJson>(cleaned, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao analisar JSON de recall de contexto retornado pelo LLM Judge: {Raw}", rawResponse);
                return null;
            }
        }

        private class ContextRecallResponseJson
        {
            public double Score { get; set; }
            public List<string> Reasoning { get; set; } = new List<string>();
        }
    }
}
