using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Adapters
{
    /// <summary>
    /// Adaptador flexível que simula o envio de mensagens contra APIs customizadas, webchats de terceiros e webhooks proprietários.
    /// </summary>
    public class CustomApiChatbotAdapter : ChatbotAdapterBase
    {
        public CustomApiChatbotAdapter(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public override string ChannelType => "ChatbotAPI"; // Alinhado com o comentário da entidade AgentConfig no banco

        /// <summary>
        /// Monta o payload HTTP genérico estruturado para APIs proprietárias.
        /// </summary>
        protected override HttpRequestMessage BuildRequest(string endpoint, string apiKey, string message)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Injetar token de validação de forma abrangente em cabeçalhos comuns (Bearer e X-Api-Key)
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);
            }

            // Estrutura de dados genérica amplamente consumida por backends de chats proprietários
            var customPayload = new
            {
                message = message,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var json = JsonSerializer.Serialize(customPayload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }

        /// <summary>
        /// Analisa o retorno do bot, decodificando formatos JSON ou consumindo texto plano bruto de forma resiliente.
        /// </summary>
        protected override async Task<string> ParseResponseAsync(HttpResponseMessage response)
        {
            var rawContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawContent))
                return "A API do chatbot processou a simulação com sucesso, mas o corpo de retorno está vazio.";

            try
            {
                // Tenta extrair a resposta do JSON estruturado caso o webhook proprietário responda com objetos
                using var jsonDoc = JsonDocument.Parse(rawContent);
                var root = jsonDoc.RootElement;

                // Tenta mapear chaves de texto genéricas comuns em respostas estruturadas de IA
                if (root.TryGetProperty("response", out var respProp)) return respProp.GetString() ?? rawContent;
                if (root.TryGetProperty("text", out var textProp)) return textProp.GetString() ?? rawContent;
                if (root.TryGetProperty("message", out var msgProp)) return msgProp.GetString() ?? rawContent;
                if (root.TryGetProperty("reply", out var replyProp)) return replyProp.GetString() ?? rawContent;

                return rawContent;
            }
            catch
            {
                // Se a resposta for em texto puro, retorna diretamente
                return rawContent;
            }
        }
    }
}
