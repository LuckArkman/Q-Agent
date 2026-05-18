using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Database.Context;
using Database.Entities;
using Repositorys.Interfaces;

namespace Repositorys.Implementations
{
    /// <summary>
    /// Repositório concreto para persistência e consulta da entidade TestSuite, incluindo carregamento síncrono e assíncrono de dependências.
    /// </summary>
    public class TestSuiteRepository : GenericRepository<TestSuite>, ITestSuiteRepository
    {
        public TestSuiteRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<TestSuite?> GetTestSuiteWithCasesAsync(Guid id)
        {
            return await _dbSet
                .Include(ts => ts.TestCases)
                .FirstOrDefaultAsync(ts => ts.Id == id);
        }
    }
}
