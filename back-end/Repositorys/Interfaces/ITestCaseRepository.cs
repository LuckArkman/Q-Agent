using Database.Entities;

namespace Repositorys.Interfaces
{
    /// <summary>
    /// Abstração para operações específicas de persistência e consulta da entidade TestCase.
    /// </summary>
    public interface ITestCaseRepository : IGenericRepository<TestCase>
    {
    }
}
