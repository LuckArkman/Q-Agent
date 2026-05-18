using Database.Context;
using Database.Entities;
using Repositorys.Interfaces;

namespace Repositorys.Implementations
{
    /// <summary>
    /// Repositório concreto para persistência e consulta da entidade TestCase.
    /// </summary>
    public class TestCaseRepository : GenericRepository<TestCase>, ITestCaseRepository
    {
        public TestCaseRepository(AppDbContext context) : base(context)
        {
        }
    }
}
