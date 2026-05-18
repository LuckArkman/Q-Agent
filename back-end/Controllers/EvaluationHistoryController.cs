using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Database.Mongo;
using Dtos.History;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace Controllers
{
    /// <summary>
    /// Controller responsável por expor as rotas HTTP de consulta paginada de históricos de auditoria do MongoDB.
    /// </summary>
    [ApiController]
    [Route("api/history")]
    public class EvaluationHistoryController : ControllerBase
    {
        private readonly MongoContext _mongoContext;
        private readonly ILogger<EvaluationHistoryController> _logger;

        public EvaluationHistoryController(MongoContext mongoContext, ILogger<EvaluationHistoryController> logger)
        {
            _mongoContext = mongoContext ?? throw new ArgumentNullException(nameof(mongoContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Recupera o histórico de auditorias de simulação no MongoDB, aplicando paginação e filtros opcionais.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistoryDetailDto>>> GetHistory([FromQuery] HistoryFilterDto filter)
        {
            // Validações básicas de paginação para evitar estouro de memória
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize); // Limite de segurança de 100 registros por página

            _logger.LogInformation("Consulta paginada ao histórico MongoDB. Página: {Page} | Registros: {PageSize}", page, pageSize);

            try
            {
                var builder = Builders<EvaluationHistory>.Filter;
                var mongoFilter = builder.Empty;

                if (filter.AgentConfigId.HasValue)
                {
                    mongoFilter &= builder.Eq(h => h.AgentConfigId, filter.AgentConfigId.Value);
                }

                if (filter.TestSuiteId.HasValue)
                {
                    mongoFilter &= builder.Eq(h => h.TestSuiteId, filter.TestSuiteId.Value);
                }

                if (filter.MinScore.HasValue)
                {
                    mongoFilter &= builder.Gte(h => h.GeneralQualityScore, filter.MinScore.Value);
                }

                // Cálculo de paginação e ordenação decrescente por data de execução
                var skip = (page - 1) * pageSize;
                var query = _mongoContext.EvaluationHistories
                    .Find(mongoFilter)
                    .Sort(Builders<EvaluationHistory>.Sort.Descending(h => h.ExecutedAt))
                    .Skip(skip)
                    .Limit(pageSize);

                var documents = await query.ToListAsync();
                var result = new List<HistoryDetailDto>();

                foreach (var doc in documents)
                {
                    result.Add(new HistoryDetailDto
                    {
                        Id = doc.Id,
                        AgentConfigId = doc.AgentConfigId,
                        TestSuiteId = doc.TestSuiteId,
                        TestCaseId = doc.TestCaseId,
                        SentPrompt = doc.SentPrompt,
                        ReceivedResponse = doc.ReceivedResponse,
                        LatencyMs = doc.LatencyMs,
                        StatusCode = doc.StatusCode,
                        FaithfulnessScore = doc.FaithfulnessScore,
                        AnswerRelevanceScore = doc.AnswerRelevanceScore,
                        ContextPrecisionScore = doc.ContextPrecisionScore,
                        ContextRecallScore = doc.ContextRecallScore,
                        GeneralQualityScore = doc.GeneralQualityScore,
                        JudgmentReasoning = doc.JudgmentReasoning,
                        ModelUsedForJudge = doc.ModelUsedForJudge,
                        ExecutedAt = doc.ExecutedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de processamento na consulta paginada ao histórico de auditorias no MongoDB.");
                return StatusCode(500, new { Message = "Erro crítico interno ao consultar o histórico no MongoDB." });
            }
        }

        /// <summary>
        /// Recupera o laudo e trilha técnica detalhada de uma auditoria específica através do seu ObjectId do MongoDB.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<HistoryDetailDto>> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length != 24) // Validação padrão de formato ObjectId hexadecimal do MongoDB
            {
                return BadRequest(new { Message = "O identificador informado não está em um formato válido de ObjectId do MongoDB." });
            }

            _logger.LogInformation("Consulta de detalhe de histórico MongoDB. ObjectId: '{Id}'", id);

            try
            {
                var doc = await _mongoContext.EvaluationHistories.Find(h => h.Id == id).FirstOrDefaultAsync();
                if (doc == null)
                {
                    _logger.LogWarning("Histórico MongoDB '{Id}' não localizado.", id);
                    return NotFound(new { Message = $"Laudo de auditoria com ID '{id}' não encontrado no MongoDB." });
                }

                var dto = new HistoryDetailDto
                {
                    Id = doc.Id,
                    AgentConfigId = doc.AgentConfigId,
                    TestSuiteId = doc.TestSuiteId,
                    TestCaseId = doc.TestCaseId,
                    SentPrompt = doc.SentPrompt,
                    ReceivedResponse = doc.ReceivedResponse,
                    LatencyMs = doc.LatencyMs,
                    StatusCode = doc.StatusCode,
                    FaithfulnessScore = doc.FaithfulnessScore,
                    AnswerRelevanceScore = doc.AnswerRelevanceScore,
                    ContextPrecisionScore = doc.ContextPrecisionScore,
                    ContextRecallScore = doc.ContextRecallScore,
                    GeneralQualityScore = doc.GeneralQualityScore,
                    JudgmentReasoning = doc.JudgmentReasoning,
                    ModelUsedForJudge = doc.ModelUsedForJudge,
                    ExecutedAt = doc.ExecutedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de processamento na consulta ao histórico '{Id}' no MongoDB.", id);
                return StatusCode(500, new { Message = "Erro crítico interno ao obter laudo de auditoria." });
            }
        }
    }
}
