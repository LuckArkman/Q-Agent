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
    /// Serviço de auditoria conversacional que analisa se as fatias RAG retornadas pela busca vetorial no ChromaDB são de fato úteis e se estão ordenadas corretamente por relevância.
    /// </summary>
    public class ContextPrecisionEvaluator : IContextPrecisionEvaluator
    {
        private readonly ILlmJudgeClient _llmJudgeClient;
        private readonly ILogger<ContextPrecisionEvaluator> _logger;

        public ContextPrecisionEvaluator(ILlmJudgeClient llmJudgeClient, ILogger<ContextPrecisionEvaluator> logger)
        {
            _llmJudgeClient = llmJudgeClient ?? throw new ArgumentNullException(nameof(llmJudgeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Compara os blocos de RAG trazidos com a pergunta do usuário e calcula a pontuação de ranqueamento e precisão (Context Precision).
        /// </summary>
        public async Task<ContextPrecisionResultDto> EvaluateContextPrecisionAsync(string prompt, List<RagContextChunkDto> contextChunks)
        {
            var result = new ContextPrecisionResultDto
            {
                Score = 0.0,
                IsPrecise = false,
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
                    "Sua missão é classificar a PRECISÃO DO CONTEXTO (Context Precision) de um conjunto de fatias de contexto RAG recuperadas pelo mecanismo de busca vetorial.\n" +
                    "O objetivo é avaliar se as fatias trazidas pelo banco vetorial são REALMENTE ÚTEIS e pertinentes para responder à Pergunta do Usuário, e se as fatias mais importantes estão corretamente posicionadas no topo (ranqueamento de alta relevância).\n\n" +
                    "Diretrizes:\n" +
                    "- Avalie cada fatia individualmente e classifique se ela é relevante (útil) ou irrelevante (ruído) para responder à Pergunta.\n" +
                    "- O score de precisão deve ser um valor de 0.0 a 1.0, calculando a fração de fatias úteis em relação ao total, ponderando a importância da ordenação (as fatias úteis devem vir primeiro).\n" +
                    "- Você DEVE retornar estritamente um JSON estruturado com o seguinte layout:\n" +
                    "{\n" +
                    "  \"score\": 0.85,\n" +
                    "  \"reasoning\": [\n" +
                    "    \"A Fatia 1 é altamente relevante e fornece a informação X.\",\n" +
                    "    \"A Fatia 2 é ruído ou redundante, reduzindo o score final.\"\n" +
                    "  ]\n" +
                    "}\n" +
                    "Não inclua nenhum texto de abertura ou fechamento, responda apenas com o JSON puro.";

                // 3. Estabelecer o Prompt do Usuário
                var userPromptBuilder = new StringBuilder();
                userPromptBuilder.AppendLine("=== PERGUNTA DO USUÁRIO ===");
                userPromptBuilder.AppendLine(prompt);
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("=== FATIAS RECUPERADAS (ORDEM DE RETORNO DO BANCO VETORIAL) ===");
                userPromptBuilder.AppendLine(contextBuilder.ToString());
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("Avalie a precisão de contexto, gerando o JSON:");

                // 4. Despachar para a API do LLM Judge
                var rawLlmResponse = await _llmJudgeClient.ExecuteEvaluationAsync(systemPrompt, userPromptBuilder.ToString());
                if (string.IsNullOrWhiteSpace(rawLlmResponse))
                {
                    _logger.LogWarning("O LLM Judge retornou corpo em branco para precisão de contexto. Atribuindo nota mínima.");
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
                    result.IsPrecise = result.Score >= 0.8; // BOAS PRÁTICAS: 80% como score mínimo de aprovação de ranqueamento preciso
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
                _logger.LogError(ex, "Erro na execução da avaliação de precisão de contexto.");
                result.Reasoning.Add($"Erro interno do avaliador: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Limpa o corpo textual do JSON para extração à prova de falhas.
        /// </summary>
        private ContextPrecisionResponseJson? ParseLlmResponse(string rawResponse)
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

                return JsonSerializer.Deserialize<ContextPrecisionResponseJson>(cleaned, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao analisar JSON de precisão de contexto retornado pelo LLM Judge: {Raw}", rawResponse);
                return null;
            }
        }

        private class ContextPrecisionResponseJson
        {
            public double Score { get; set; }
            public List<string> Reasoning { get; set; } = new List<string>();
        }
    }
}
