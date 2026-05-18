using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Mongo;

namespace Repositorys.Interfaces
{
    /// <summary>
    /// Abstração para operações específicas de persistência e consulta do histórico de avaliações (MongoDB).
    /// </summary>
    public interface IEvaluationHistoryRepository
    {
        Task<EvaluationHistory?> GetByIdAsync(string id);
        Task<IEnumerable<EvaluationHistory>> GetAllAsync();
        Task<IEnumerable<EvaluationHistory>> GetByAgentIdAsync(Guid agentId);
        Task<IEnumerable<EvaluationHistory>> GetByTestSuiteIdAsync(Guid testSuiteId);
        Task AddAsync(EvaluationHistory history);
    }
}
