using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Database.Mongo;
using Dtos.Analytics;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Services.Cache;

namespace Controllers
{
    /// <summary>
    /// Controller responsável por expor rotas de agregação analítica e evolução temporal de métricas RAG para dashboards com suporte a Cache.
    /// </summary>
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly MongoContext _mongoContext;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(MongoContext mongoContext, ICacheService cacheService, ILogger<AnalyticsController> logger)
        {
            _mongoContext = mongoContext ?? throw new ArgumentNullException(nameof(mongoContext));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retorna os KPIs gerais e indicadores estatísticos para os cartões de cabeçalho do dashboard (com Cache híbrido).
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<DashboardMetricsDto>> GetDashboardMetrics([FromQuery] Guid? agentConfigId, [FromQuery] Guid? testSuiteId)
        {
            var cacheKey = $"dashboard:metrics:agent_{(agentConfigId?.ToString() ?? "all")}:suite_{(testSuiteId?.ToString() ?? "all")}";
            
            try
            {
                // Tenta recuperar do cache de alto rendimento
                var cachedMetrics = await _cacheService.GetAsync<DashboardMetricsDto>(cacheKey);
                if (cachedMetrics != null)
                {
                    _logger.LogInformation("Retornando indicadores consolidados a partir do cache. Chave: {Key}", cacheKey);
                    return Ok(cachedMetrics);
                }

                _logger.LogInformation("Calculando indicadores consolidados para o dashboard (Cache Miss).");

                var builder = Builders<EvaluationHistory>.Filter;
                var mongoFilter = builder.Empty;

                if (agentConfigId.HasValue)
                {
                    mongoFilter &= builder.Eq(h => h.AgentConfigId, agentConfigId.Value);
                }

                if (testSuiteId.HasValue)
                {
                    mongoFilter &= builder.Eq(h => h.TestSuiteId, testSuiteId.Value);
                }

                // Conta total de simulações físicas correspondentes aos filtros
                var totalExecutions = await _mongoContext.EvaluationHistories.CountDocumentsAsync(mongoFilter);

                if (totalExecutions == 0)
                {
                    return Ok(new DashboardMetricsDto());
                }

                // EXCELÊNCIA TÉCNICA: Agregação matemática que ignora campos textuais longos (raciocínio/prompts)
                var pipeline = _mongoContext.EvaluationHistories.Aggregate()
                    .Match(mongoFilter)
                    .Group(h => 1, g => new
                    {
                        AvgFaithfulness = g.Average(h => h.FaithfulnessScore),
                        AvgAnswerRelevance = g.Average(h => h.AnswerRelevanceScore),
                        AvgContextPrecision = g.Average(h => h.ContextPrecisionScore),
                        AvgContextRecall = g.Average(h => h.ContextRecallScore),
                        AvgGeneralQuality = g.Average(h => h.GeneralQualityScore),
                        AvgLatency = g.Average(h => h.LatencyMs)
                    });

                var stats = await pipeline.FirstOrDefaultAsync();

                // Calcula quantidade de status HTTP 200 (Sucesso)
                var successFilter = mongoFilter & builder.Eq(h => h.StatusCode, 200);
                var successExecutions = await _mongoContext.EvaluationHistories.CountDocumentsAsync(successFilter);

                var successRate = totalExecutions > 0 
                    ? Math.Round(((double)successExecutions / totalExecutions) * 100.0, 2) 
                    : 100.0;

                var result = new DashboardMetricsDto
                {
                    TotalExecutions = totalExecutions,
                    AverageFaithfulness = stats != null ? Math.Round(stats.AvgFaithfulness, 4) : 0.0,
                    AverageAnswerRelevance = stats != null ? Math.Round(stats.AvgAnswerRelevance, 4) : 0.0,
                    AverageContextPrecision = stats != null ? Math.Round(stats.AvgContextPrecision, 4) : 0.0,
                    AverageContextRecall = stats != null ? Math.Round(stats.AvgContextRecall, 4) : 0.0,
                    AverageGeneralQuality = stats != null ? Math.Round(stats.AvgGeneralQuality, 4) : 0.0,
                    AverageLatencyMs = stats != null ? Math.Round(stats.AvgLatency, 2) : 0.0,
                    SuccessRate = successRate
                };

                // Armazena no cache pelo prazo de 5 minutos
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de processamento ao calcular métricas do dashboard no MongoDB.");
                return StatusCode(500, new { Message = "Erro crítico interno ao gerar indicadores de qualidade." });
            }
        }

        /// <summary>
        /// Retorna a evolução temporal diária das médias dos scores RAG filtrada e agrupada para gráficos lineares (com Cache híbrido).
        /// </summary>
        [HttpGet("timeline")]
        public async Task<ActionResult<TimelineChartDto>> GetTimelineChart([FromQuery] Guid? agentConfigId, [FromQuery] Guid? testSuiteId, [FromQuery] int days = 30)
        {
            var cacheKey = $"dashboard:timeline:agent_{(agentConfigId?.ToString() ?? "all")}:suite_{(testSuiteId?.ToString() ?? "all")}:days_{days}";

            try
            {
                // Tenta recuperar do cache de alto rendimento
                var cachedTimeline = await _cacheService.GetAsync<TimelineChartDto>(cacheKey);
                if (cachedTimeline != null)
                {
                    _logger.LogInformation("Retornando série temporal das médias RAG a partir do cache. Chave: {Key}", cacheKey);
                    return Ok(cachedTimeline);
                }

                _logger.LogInformation("Calculando série temporal RAG (Cache Miss).");

                var cutoffDate = DateTime.UtcNow.AddDays(-Math.Abs(days));
                var builder = Builders<EvaluationHistory>.Filter;
                var mongoFilter = builder.Gte(h => h.ExecutedAt, cutoffDate);

                if (agentConfigId.HasValue)
                {
                    mongoFilter &= builder.Eq(h => h.AgentConfigId, agentConfigId.Value);
                }

                if (testSuiteId.HasValue)
                {
                    mongoFilter &= builder.Eq(h => h.TestSuiteId, testSuiteId.Value);
                }

                // EXCELÊNCIA TÉCNICA: Descarta prompts e respostas longas diretamente no banco usando Projeção.
                var rawData = await _mongoContext.EvaluationHistories
                    .Find(mongoFilter)
                    .Project(h => new
                    {
                        DateKey = new DateTime(h.ExecutedAt.Year, h.ExecutedAt.Month, h.ExecutedAt.Day),
                        h.FaithfulnessScore,
                        h.AnswerRelevanceScore,
                        h.ContextPrecisionScore,
                        h.ContextRecallScore,
                        h.GeneralQualityScore
                    })
                    .ToListAsync();

                // Agrupamento na memória local
                var grouped = rawData.GroupBy(r => r.DateKey)
                    .Select(g => new TimelinePointDto
                    {
                        DateLabel = g.Key.ToString("yyyy-MM-dd"),
                        ExecutionCount = g.Count(),
                        AverageGeneralQuality = Math.Round(g.Average(x => x.GeneralQualityScore), 4),
                        AverageFaithfulness = Math.Round(g.Average(x => x.FaithfulnessScore), 4),
                        AverageAnswerRelevance = Math.Round(g.Average(x => x.AnswerRelevanceScore), 4),
                        AverageContextPrecision = Math.Round(g.Average(x => x.ContextPrecisionScore), 4),
                        AverageContextRecall = Math.Round(g.Average(x => x.ContextRecallScore), 4)
                    })
                    .OrderBy(p => p.DateLabel)
                    .ToList();

                var result = new TimelineChartDto { Points = grouped };

                // Armazena no cache pelo prazo de 5 minutos
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de processamento ao compor evolução temporal de qualidade semântica.");
                return StatusCode(500, new { Message = "Erro crítico de servidor ao compor linha do tempo analítica." });
            }
        }
    }
}
