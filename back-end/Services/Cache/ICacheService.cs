using System;
using System.Threading.Tasks;

namespace Services.Cache
{
    /// <summary>
    /// Contrato de serviço para gerenciamento de cache de alta performance (Memória Local / Redis).
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Recupera um elemento do cache e o desserializa no formato de dados de destino.
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Armazena ou substitui um elemento serializado no cache com tempo de expiração customizado.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Invalida e remove uma chave específica do cache físico.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Invalida em lote chaves de cache correspondentes a um prefixo ou padrão (Ex: "dashboard:*").
        /// </summary>
        Task RemoveByPrefixAsync(string prefix);
    }
}
