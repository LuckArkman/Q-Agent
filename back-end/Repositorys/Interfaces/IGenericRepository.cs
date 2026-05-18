using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositorys.Interfaces
{
    /// <summary>
    /// Interface genérica para persistência e consulta básica de dados.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade de banco de dados.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> SaveChangesAsync();
    }
}
