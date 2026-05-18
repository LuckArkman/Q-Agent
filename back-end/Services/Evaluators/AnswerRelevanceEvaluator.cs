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
    /// Serviço de auditoria conversacional que analisa se a resposta do chatbot atende diretamente à dúvida original do usuário ou se é evasiva, genérica ou off-topic.
    /// </summary>
    public class AnswerRelevanceEvaluator : IAnswerRelevanceEvaluator
    {
        private readonly ILlmJudgeClient _llmJudgeClient;
        private readonly ILogger<AnswerRelevanceEvaluator> _logger;

        public AnswerRelevanceEvaluator(ILlmJudgeClient llmJudgeClient, ILogger<AnswerRelevanceEvaluator> logger)
        {
            _llmJudgeClient = llmJudgeClient ?? throw new ArgumentNullException(nameof(llmJudgeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Compara a resposta gerada com a pergunta do usuário e extrai a pontuação de relevância (Answer Relevance).
        /// </summary>
        public async Task<AnswerRelevanceResultDto> EvaluateAnswerRelevanceAsync(string prompt, string response, string? contextDescription = null)
        {
            var result = new AnswerRelevanceResultDto
            {
                Score = 0.0,
                IsRelevant = false,
                RawLlmResponse = string.Empty
            };

            try
            {
                // 1. Estabelecer o Prompt de Sistema com diretrizes para o LLM Judge
                var systemPrompt = 
                    "Você é um auditor de qualidade e engenheiro de testes especializado em inteligência artificial conversacional.\n" +
                    "Sua missão é classificar a RELEVÂNCIA (Answer Relevance) de uma resposta gerada pelo Chatbot contra a Pergunta original do usuário.\n" +
                    "O objetivo é medir o quanto a resposta atende e resolve a dúvida diretamente, penalizando respostas genéricas, evasivas (ex: 'Não sei como ajudar', 'Desculpe, não posso fazer isso') quando a pergunta é legítima, ou respostas que dão voltas (tangenciam) sem responder.\n\n" +
                    "Diretrizes:\n" +
                    "- Avalie se a resposta é focada e direta ou se inclui redundâncias.\n" +
                    "- Penalize severamente respostas vazias ou recusas evasivas genéricas.\n" +
                    "- Atribua um score entre 0.0 e 1.0:\n" +
                    "  * 1.0: A resposta é perfeita, responde diretamente e resolve por completo a dúvida do usuário.\n" +
                    "  * 0.0: A resposta é totalmente irrelevante, vazia, off-topic ou evasiva extrema.\n" +
                    "- Você DEVE retornar estritamente um JSON estruturado com o seguinte layout:\n" +
                    "{\n" +
                    "  \"score\": 0.95,\n" +
                    "  \"reasoning\": [\n" +
                    "    \"A resposta aborda diretamente o cerne da pergunta.\",\n" +
                    "    \"Não houve tangenciamento ou recusa evasiva.\"\n" +
                    "  ]\n" +
                    "}\n" +
                    "Não inclua nenhum texto de abertura ou fechamento, responda apenas com o JSON puro.";

                // 2. Estabelecer o Prompt do Usuário
                var userPromptBuilder = new StringBuilder();
                userPromptBuilder.AppendLine("=== PERGUNTA DO USUÁRIO ===");
                userPromptBuilder.AppendLine(prompt);
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("=== RESPOSTA GERADA PELO CHATBOT ===");
                userPromptBuilder.AppendLine(response);
                userPromptBuilder.AppendLine();
                if (!string.IsNullOrWhiteSpace(contextDescription))
                {
                    userPromptBuilder.AppendLine("=== CONTEXTO ADICIONAL ===");
                    userPromptBuilder.AppendLine(contextDescription);
                    userPromptBuilder.AppendLine();
                }
                userPromptBuilder.AppendLine("Avalie a relevância da resposta gerada, gerando o JSON:");

                // 3. Despachar para a API do LLM Judge
                var rawLlmResponse = await _llmJudgeClient.ExecuteEvaluationAsync(systemPrompt, userPromptBuilder.ToString());
                if (string.IsNullOrWhiteSpace(rawLlmResponse))
                {
                    _logger.LogWarning("O LLM Judge retornou corpo em branco para relevância. Atribuindo nota mínima.");
                    result.Reasoning.Add("A IA juíza falhou em responder.");
                    return result;
                }

                result.RawLlmResponse = rawLlmResponse;

                // 4. Parsing do JSON de forma resiliente
                var parsedResult = ParseLlmResponse(rawLlmResponse);
                if (parsedResult != null)
                {
                    result.Score = parsedResult.Score;
                    result.Reasoning = parsedResult.Reasoning;
                    result.IsRelevant = result.Score >= 0.8; // BOAS PRÁTICAS: 80% como score mínimo de aprovação útil
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
                _logger.LogError(ex, "Erro na execução da avaliação de relevância de resposta.");
                result.Reasoning.Add($"Erro interno do avaliador: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Limpa o corpo textual do JSON para extração à prova de falhas.
        /// </summary>
        private AnswerRelevanceResponseJson? ParseLlmResponse(string rawResponse)
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

                return JsonSerializer.Deserialize<AnswerRelevanceResponseJson>(cleaned, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao analisar JSON de relevância retornado pelo LLM Judge: {Raw}", rawResponse);
                return null;
            }
        }

        private class AnswerRelevanceResponseJson
        {
            public double Score { get; set; }
            public List<string> Reasoning { get; set; } = new List<string>();
        }
    }
}
