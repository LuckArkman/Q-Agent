using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Adapters
{
    /// <summary>
    /// Classe base que governa o comportamento comum, resiliência e cálculo puro de latência em milissegundos para simulações de chatbots.
    /// </summary>
    public abstract class ChatbotAdapterBase : IChatbotAdapter
    {
        protected readonly IHttpClientFactory HttpClientFactory;

        protected ChatbotAdapterBase(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// O tipo de canal específico suportado pelo adaptador.
        /// </summary>
        public abstract string ChannelType { get; }

        /// <summary>
        /// Timeout limite padrão para simulações do canal.
        /// </summary>
        protected virtual TimeSpan Timeout => TimeSpan.FromSeconds(10);

        /// <summary>
        /// Envia uma mensagem simulada de teste ao bot e retorna a resposta com medição isolada e precisa de latência física.
        /// </summary>
        public async Task<ChatbotResponseDto> SendMessageAsync(string endpoint, string apiKey, string message)
        {
            var responseDto = new ChatbotResponseDto();

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                responseDto.IsSuccess = false;
                responseDto.ErrorMessage = "O endpoint de destino do chatbot está nulo ou vazio.";
                return responseDto;
            }

            try
            {
                // Construir a requisição específica de payload do canal herdado
                using var request = BuildRequest(endpoint, apiKey, message);

                using var client = HttpClientFactory.CreateClient();
                client.Timeout = Timeout;

                var stopwatch = new Stopwatch();

                try
                {
                    // BOAS PRÁTICAS: Iniciar o stopwatch exatamente antes do envio físico para isolar overheads internos de construção
                    stopwatch.Start();
                    var httpResponse = await client.SendAsync(request);
                    stopwatch.Stop(); // Parar imediatamente no retorno do socket

                    responseDto.LatencyMs = stopwatch.ElapsedMilliseconds;

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        responseDto.IsSuccess = true;
                        responseDto.ResponseText = await ParseResponseAsync(httpResponse);
                    }
                    else
                    {
                        responseDto.IsSuccess = false;
                        responseDto.ErrorMessage = $"O chatbot retornou código HTTP de erro: {(int)httpResponse.StatusCode} ({httpResponse.StatusCode}).";
                    }
                }
                catch (Exception innerEx)
                {
                    stopwatch.Stop();
                    responseDto.IsSuccess = false;
                    responseDto.LatencyMs = stopwatch.ElapsedMilliseconds;
                    responseDto.ErrorMessage = $"Erro de handshake físico de rede durante a simulação: {innerEx.Message}";
                }
            }
            catch (Exception ex)
            {
                responseDto.IsSuccess = false;
                responseDto.ErrorMessage = $"Erro crítico ao formatar a requisição de simulação: {ex.Message}";
            }

            return responseDto;
        }

        /// <summary>
        /// Assinatura abstrata para construir a requisição HTTP e injetar as credenciais específicas de cada bot/canal.
        /// </summary>
        protected abstract HttpRequestMessage BuildRequest(string endpoint, string apiKey, string message);

        /// <summary>
        /// Assinatura abstrata para analisar e extrair a resposta do bot a partir do payload de retorno.
        /// </summary>
        protected abstract Task<string> ParseResponseAsync(HttpResponseMessage response);
    }
}
