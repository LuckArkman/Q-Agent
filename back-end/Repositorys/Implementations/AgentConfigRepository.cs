using Database.Context;
using Database.Entities;
using Repositorys.Interfaces;

namespace Repositorys.Implementations
{
    /// <summary>
    /// Repositório concreto para persistência e consulta da entidade AgentConfig.
    /// </summary>
    public class AgentConfigRepository : GenericRepository<AgentConfig>, IAgentConfigRepository
    {
        public AgentConfigRepository(AppDbContext context) : base(context)
        {
        }
    }
}
