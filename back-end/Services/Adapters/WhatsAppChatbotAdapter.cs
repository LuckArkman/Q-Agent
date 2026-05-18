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
    /// Adaptador específico que simula o envio de mensagens do usuário no formato real da WhatsApp Business Cloud API.
    /// </summary>
    public class WhatsAppChatbotAdapter : ChatbotAdapterBase
    {
        public WhatsAppChatbotAdapter(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public override string ChannelType => "WhatsApp";

        /// <summary>
        /// Monta o payload HTTP no formato oficial do Webhook do WhatsApp Business API.
        /// </summary>
        protected override HttpRequestMessage BuildRequest(string endpoint, string apiKey, string message)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Adicionar token Bearer se a chave estiver configurada
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            // Estrutura exata do JSON que o WhatsApp Business Cloud envia ao receber mensagens de texto
            var whatsappWebhookPayload = new
            {
                @object = "whatsapp_business_account",
                entry = new[]
                {
                    new
                    {
                        id = "wa_biz_account_simulated",
                        changes = new[]
                        {
                            new
                            {
                                value = new
                                {
                                    messaging_product = "whatsapp",
                                    metadata = new
                                    {
                                        display_phone_number = "5511999999999",
                                        phone_number_id = "phone_id_simulated"
                                    },
                                    contacts = new[]
                                    {
                                        new
                                        {
                                            profile = new
                                            {
                                                name = "Q-Agent Simulator User"
                                            },
                                            wa_id = "5511999999999"
                                        }
                                    },
                                    messages = new[]
                                    {
                                        new
                                        {
                                            from = "5511999999999",
                                            id = $"wamid.simulated_{Guid.NewGuid():N}",
                                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                                            text = new
                                            {
                                                body = message // Suporta nativamente emojis e acentuação via codificação UTF-8
                                            },
                                            type = "text"
                                        }
                                    }
                                },
                                field = "messages"
                            }
                        }
                    }
                }
            };

            // Serializa o payload garantindo codificação UTF-8 para suporte a emojis/caracteres especiais
            var json = JsonSerializer.Serialize(whatsappWebhookPayload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }

        /// <summary>
        /// Processa a resposta do bot destino de forma flexível.
        /// </summary>
        protected override async Task<string> ParseResponseAsync(HttpResponseMessage response)
        {
            var rawContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawContent))
                return "O chatbot processou a simulação com sucesso, mas o corpo de retorno está vazio.";

            try
            {
                // Tenta extrair valores caso o webhook do chatbot responda em formato JSON estruturado
                using var jsonDoc = JsonDocument.Parse(rawContent);
                var root = jsonDoc.RootElement;

                // Tenta buscar chaves de texto comuns mapeadas em backends de chatbots
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
