using System.Threading.Tasks;
using Database.Entities;

namespace Repositorys.Interfaces
{
    /// <summary>
    /// Abstração para operações específicas de persistência e consulta da entidade UserAccount (PostgreSQL).
    /// </summary>
    public interface IUserAccountRepository : IGenericRepository<UserAccount>
    {
        /// <summary>
        /// Obtém uma conta de usuário buscando pelo nome de usuário.
        /// </summary>
        Task<UserAccount?> GetByUsernameAsync(string username);

        /// <summary>
        /// Obtém uma conta de usuário buscando pelo e-mail.
        /// </summary>
        Task<UserAccount?> GetByEmailAsync(string email);
    }
}
