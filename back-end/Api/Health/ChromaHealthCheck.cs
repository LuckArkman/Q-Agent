using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Database.Chroma;

namespace Api.Health
{
    /// <summary>
    /// Verificação de saúde customizada para validar a conectividade e integridade do ChromaDB.
    /// </summary>
    public class ChromaHealthCheck : IHealthCheck
    {
        private readonly IChromaClient _chromaClient;

        public ChromaHealthCheck(IChromaClient chromaClient)
        {
            _chromaClient = chromaClient ?? throw new ArgumentNullException(nameof(chromaClient));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = await _chromaClient.IsHealthyAsync();
                if (isHealthy)
                {
                    return HealthCheckResult.Healthy("A conectividade com o ChromaDB está totalmente íntegra.");
                }
                return HealthCheckResult.Unhealthy("O ChromaDB respondeu com status de erro ao tentar acessar o heartbeat.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Falha de conexão física ao tentar alcançar o ChromaDB.", ex);
            }
        }
    }
}
