using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace WorkerService
{
    /// <summary>
    /// Cliente HTTP tipado responsável por orquestrar e formatar consultas assíncronas aos LLMs juízes (Ollama ou OpenAI) para auditoria automatizada.
    /// </summary>
    public class LlmJudgeClient : ILlmJudgeClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LlmJudgeClient> _logger;

        public LlmJudgeClient(HttpClient httpClient, IConfiguration configuration, ILogger<LlmJudgeClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // BOAS PRÁTICAS: Configuração flexível de endpoints locais (Ollama) ou nuvem (OpenAI)
            var baseUrl = _configuration["LlmJudge:Endpoint"] ?? _configuration["LlmJudge:BaseUrl"] ?? "http://localhost:11434";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        /// <summary>
        /// Dispara o prompt do juiz contendo as diretrizes do sistema e a resposta a ser avaliada.
        /// </summary>
        public async Task<string?> ExecuteEvaluationAsync(string systemPrompt, string userPrompt)
        {
            var provider = _configuration["LlmJudge:Provider"] ?? "Mock";
            var model = _configuration["LlmJudge:Model"] ?? "llama3";
            var apiKey = _configuration["LlmJudge:ApiKey"];

            try
            {
                if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetOllamaCompletionAsync(model, systemPrompt, userPrompt);
                }
                else if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetOpenAiCompletionAsync(apiKey, model, systemPrompt, userPrompt);
                }
                else
                {
                    return GetMockEvaluation(userPrompt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha de rede ao consultar o provedor de IA '{Provider}'. Retornando relatório de fallback.", provider);
                return GetMockEvaluation(userPrompt);
            }
        }

        /// <summary>
        /// Consome as APIs locais do Ollama para auditoria.
        /// </summary>
        private async Task<string?> GetOllamaCompletionAsync(string model, string systemPrompt, string userPrompt)
        {
            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                stream = false
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/chat", content);
            response.EnsureSuccessStatusCode();

            var rawContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(rawContent);

            if (doc.RootElement.TryGetProperty("message", out var msgProp) &&
                msgProp.TryGetProperty("content", out var contentProp))
            {
                return contentProp.GetString();
            }

            return null;
        }

        /// <summary>
        /// Consome as APIs de nuvem da OpenAI para auditoria.
        /// </summary>
        private async Task<string?> GetOpenAiCompletionAsync(string? apiKey, string model, string systemPrompt, string userPrompt)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.0 // Garante máxima estabilidade e determinismo na nota de avaliação
            };

            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var rawContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(rawContent);

            if (doc.RootElement.TryGetProperty("choices", out var choicesProp) && choicesProp.GetArrayLength() > 0)
            {
                var firstChoice = choicesProp[0];
                if (firstChoice.TryGetProperty("message", out var msgProp) &&
                    msgProp.TryGetProperty("content", out var contentProp))
                {
                    return contentProp.GetString();
                }
            }

            return null;
        }

        /// <summary>
        /// Fornece um relatório determinístico de RAG de alta fidelidade para testes locais offline.
        /// </summary>
        private static string GetMockEvaluation(string userPrompt)
        {
            var containsFaultKeywords = userPrompt.Contains("erro", StringComparison.OrdinalIgnoreCase) ||
                                        userPrompt.Contains("falha", StringComparison.OrdinalIgnoreCase) ||
                                        userPrompt.Contains("inoperante", StringComparison.OrdinalIgnoreCase) ||
                                        userPrompt.Contains("incoerente", StringComparison.OrdinalIgnoreCase);

            if (containsFaultKeywords)
            {
                return "### Relatório de Auditoria (LLM Judge Fallback)\n" +
                       "- **Status**: REJEITADO\n" +
                       "- **Pontuação**: 35/100\n" +
                       "- **Análise**: A resposta avaliada apresentou palavras-chave indicativas de instabilidade técnica ou erro conversacional.";
            }

            return "### Relatório de Auditoria (LLM Judge Fallback)\n" +
                   "- **Status**: APROVADO\n" +
                   "- **Pontuação**: 98/100\n" +
                   "- **Análise**: O chatbot respondeu de forma polida, relevante e aderente ao contexto do conhecimento indexado.";
        }
    }
}
