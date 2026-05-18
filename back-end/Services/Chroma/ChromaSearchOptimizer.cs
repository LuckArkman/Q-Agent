using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Microsoft.Extensions.Logging;

namespace Services.Chroma
{
    /// <summary>
    /// Classe especializada em otimizar e envelopar chamadas contra o ChromaDB usando as modernas políticas unificadas de resiliência da Polly v8.
    /// </summary>
    public class ChromaSearchOptimizer
    {
        private readonly ILogger<ChromaSearchOptimizer> _logger;
        private readonly ResiliencePipeline _resiliencePipeline;

        public ChromaSearchOptimizer(ILogger<ChromaSearchOptimizer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // BOAS PRÁTICAS: Ordem correta de resiliência: Retry (Externo) -> Circuit Breaker (Médio) -> Timeout (Interno)
            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    MaxRetryAttempts = 2,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true, // Jitter para atenuar picos de chamadas simultâneas
                    Delay = TimeSpan.FromMilliseconds(200),
                    OnRetry = args =>
                    {
                        _logger.LogWarning("Tentativa de rede {Attempt} falhou no ChromaDB. Reexecutando em {Delay}ms... Erro: {Msg}",
                            args.AttemptNumber + 1, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
                        return default;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    FailureRatio = 0.5, // Disjuntor abre se 50% das chamadas em amostragem falharem
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 4, // Número mínimo de conexões antes de avaliar
                    BreakDuration = TimeSpan.FromSeconds(5), // Circuito bloqueado por 5s antes do Half-Open
                    OnOpened = args =>
                    {
                        _logger.LogError("DISJUNTOR DO CHROMADB ABERTO! Conexões subsequentes falharão imediatamente por {Break}s.", 
                            args.BreakDuration.TotalSeconds);
                        return default;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("DISJUNTOR DO CHROMADB FECHADO. Tráfego de busca semântica normalizado.");
                        return default;
                    }
                })
                .AddTimeout(new TimeoutStrategyOptions
                {
                    // BOAS PRÁTICAS: Limite curto (3 segundos) para buscas vetoriais, impedindo travamento de threads do servidor
                    Timeout = TimeSpan.FromSeconds(3),
                    OnTimeout = args =>
                    {
                        _logger.LogWarning("Timeout de 3 segundos excedido ao consultar o ChromaDB.");
                        return default;
                    }
                })
                .Build();
        }

        /// <summary>
        /// Executa uma instrução HTTP contra o ChromaDB em um pipeline de tolerância a falhas.
        /// </summary>
        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _resiliencePipeline.ExecuteAsync(async token => await action(token), cancellationToken);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Requisição vetorial barrada pelo disjuntor (ChromaDB Offline).");
                throw;
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogError(ex, "Consulta vetorial abortada pelo timeout limite de 3 segundos.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de rede tratado pelo orquestrador resiliente do ChromaDB.");
                throw;
            }
        }
    }
}
