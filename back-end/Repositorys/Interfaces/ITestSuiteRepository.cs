using System;
using System.Threading.Tasks;
using Database.Entities;

namespace Repositorys.Interfaces
{
    /// <summary>
    /// Abstração para operações específicas de persistência e consulta da entidade TestSuite.
    /// </summary>
    public interface ITestSuiteRepository : IGenericRepository<TestSuite>
    {
        /// <summary>
        /// Obtém uma suíte de testes carregando avidamente todos os seus casos de testes associados.
        /// </summary>
        /// <param name="id">O identificador único da suíte de testes.</param>
        Task<TestSuite?> GetTestSuiteWithCasesAsync(Guid id);
    }
}
