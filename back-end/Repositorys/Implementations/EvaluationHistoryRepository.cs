using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Database.Mongo;
using Repositorys.Interfaces;

namespace Repositorys.Implementations
{
    /// <summary>
    /// Repositório concreto do MongoDB para consulta e escrita do histórico de avaliações.
    /// </summary>
    public class EvaluationHistoryRepository : IEvaluationHistoryRepository
    {
        private readonly MongoContext _mongoContext;

        public EvaluationHistoryRepository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext ?? throw new ArgumentNullException(nameof(mongoContext));
            
            try
            {
                // Inicialização idempotente de índices essenciais para performance analítica
                var agentIndex = Builders<EvaluationHistory>.IndexKeys.Ascending(h => h.AgentConfigId);
                var suiteIndex = Builders<EvaluationHistory>.IndexKeys.Ascending(h => h.TestSuiteId);
                var dateIndex = Builders<EvaluationHistory>.IndexKeys.Descending(h => h.ExecutedAt);

                _mongoContext.EvaluationHistories.Indexes.CreateOne(new CreateIndexModel<EvaluationHistory>(agentIndex));
                _mongoContext.EvaluationHistories.Indexes.CreateOne(new CreateIndexModel<EvaluationHistory>(suiteIndex));
                _mongoContext.EvaluationHistories.Indexes.CreateOne(new CreateIndexModel<EvaluationHistory>(dateIndex));
            }
            catch (Exception)
            {
                // Logar ou ignorar falhas silenciosamente caso o banco esteja inacessível na inicialização (resiliência)
            }
        }

        public async Task<EvaluationHistory?> GetByIdAsync(string id)
        {
            return await _mongoContext.EvaluationHistories
                .Find(h => h.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EvaluationHistory>> GetAllAsync()
        {
            return await _mongoContext.EvaluationHistories
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvaluationHistory>> GetByAgentIdAsync(Guid agentId)
        {
            return await _mongoContext.EvaluationHistories
                .Find(h => h.AgentConfigId == agentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvaluationHistory>> GetByTestSuiteIdAsync(Guid testSuiteId)
        {
            return await _mongoContext.EvaluationHistories
                .Find(h => h.TestSuiteId == testSuiteId)
                .ToListAsync();
        }

        public async Task AddAsync(EvaluationHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            await _mongoContext.EvaluationHistories.InsertOneAsync(history);
        }
    }
}
