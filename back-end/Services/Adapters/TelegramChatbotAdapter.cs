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
    /// Adaptador específico que simula o envio de mensagens do usuário no formato de Webhook oficial da Telegram Bot API.
    /// </summary>
    public class TelegramChatbotAdapter : ChatbotAdapterBase
    {
        public TelegramChatbotAdapter(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public override string ChannelType => "Telegram";

        /// <summary>
        /// BOAS PRÁTICAS: Define um timeout específico para o Telegram (7 segundos) para tratar com resiliência instabilidades de rede.
        /// </summary>
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(7);

        /// <summary>
        /// Monta o payload HTTP no formato oficial de Update enviado pela Telegram Bot API.
        /// </summary>
        protected override HttpRequestMessage BuildRequest(string endpoint, string apiKey, string message)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Injetar credencial no header padrão Bearer e no cabeçalho proprietário de validação do Telegram (Secret Token)
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.TryAddWithoutValidation("X-Telegram-Bot-Api-Secret-Token", apiKey);
            }

            // Estrutura padrão real de Update do Telegram contendo um objeto Message
            var telegramWebhookPayload = new
            {
                update_id = new Random().Next(10000000, 99999999),
                message = new
                {
                    message_id = new Random().Next(1000, 9999),
                    from = new
                    {
                        id = 999999999,
                        is_bot = false,
                        first_name = "Q-Agent Simulator User",
                        username = "qagent_simulator_user"
                    },
                    chat = new
                    {
                        id = 999999999,
                        first_name = "Q-Agent Simulator User",
                        username = "qagent_simulator_user",
                        type = "private"
                    },
                    date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    text = message // Suporta nativamente emojis e caracteres especiais via codificação UTF-8
                }
            };

            var json = JsonSerializer.Serialize(telegramWebhookPayload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }

        /// <summary>
        /// Processa a resposta do bot destino de forma resiliente.
        /// </summary>
        protected override async Task<string> ParseResponseAsync(HttpResponseMessage response)
        {
            var rawContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawContent))
                return "O chatbot processou a simulação com sucesso, mas o corpo de retorno está vazio.";

            try
            {
                // Tenta fazer o parse do JSON de retorno caso o chatbot responda de forma estruturada
                using var jsonDoc = JsonDocument.Parse(rawContent);
                var root = jsonDoc.RootElement;

                // Tenta extrair a mensagem a partir de chaves comuns de chatbots
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
