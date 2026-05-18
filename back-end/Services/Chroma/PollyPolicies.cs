using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace Services.Chroma
{
    /// <summary>
    /// Fornece políticas centralizadas de resiliência HTTP utilizando Polly,
    /// como retentativas com recuo exponencial e Circuit Breaker (Disjuntor) contra falhas físicas.
    /// </summary>
    public static class PollyPolicies
    {
        /// <summary>
        /// Define uma política de retentativa (Retry) com recuo exponencial (Exponential Backoff).
        /// Executa até 3 tentativas adicionais diante de falhas transientes de rede ou códigos 5xx.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Lida com falhas de rede, timeouts (HTTP 408) e erros de servidor (HTTP 5xx)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Recuo: 2s, 4s, 8s
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"[Polly Retry] Falha transiente na chamada HTTP. Tentativa #{retryAttempt} agendada em {timespan.TotalSeconds} segundos.");
                    });
        }

        /// <summary>
        /// Define uma política de disjuntor (Circuit Breaker) contra falhas físicas consecutivas.
        /// Abre o circuito após 5 falhas consecutivas, mantendo-o aberto por 30 segundos para evitar congestionamento de threads.
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5, // 5 falhas consecutivas antes de romper o circuito
                    durationOfBreak: TimeSpan.FromSeconds(30), // Mantém o circuito aberto por 30 segundos
                    onBreak: (outcome, timespan, context) =>
                    {
                        Console.WriteLine($"[Polly CircuitBreaker] O circuito foi ABERTO por {timespan.TotalSeconds} segundos devido a falhas consecutivas!");
                    },
                    onReset: (context) =>
                    {
                        Console.WriteLine("[Polly CircuitBreaker] O circuito foi FECHADO e normalizado. Conectividade restabelecida.");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("[Polly CircuitBreaker] O circuito está MEIO ABERTO. Testando conectividade com chamadas piloto.");
                    });
        }
    }
}
