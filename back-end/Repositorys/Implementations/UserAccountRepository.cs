using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Database.Context;
using Database.Entities;
using Repositorys.Interfaces;

namespace Repositorys.Implementations
{
    /// <summary>
    /// Repositório concreto do PostgreSQL para gerenciamento de contas de usuário.
    /// </summary>
    public class UserAccountRepository : GenericRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserAccount?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            var normalized = username.ToLowerInvariant();
            return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
        }

        public async Task<UserAccount?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalized = email.ToLowerInvariant();
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
        }
    }
}
