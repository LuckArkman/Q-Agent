using System.Threading.Tasks;

namespace Database.Chroma
{
    /// <summary>
    /// Define as operações HTTP básicas para conectar e interagir com o banco de dados vetorial ChromaDB.
    /// </summary>
    public interface IChromaClient
    {
        /// <summary>
        /// Verifica se a API do ChromaDB está online e saudável (Heartbeat).
        /// </summary>
        Task<bool> IsHealthyAsync();

        /// <summary>
        /// Cria uma coleção vetorial no ChromaDB.
        /// </summary>
        /// <param name="name">Nome da coleção.</param>
        Task<string?> CreateCollectionAsync(string name);

        /// <summary>
        /// Realiza uma busca semântica aproximada por vizinhos mais próximos baseado em embeddings.
        /// </summary>
        /// <param name="collectionId">Identificador único da coleção no ChromaDB.</param>
        /// <param name="queryEmbedding">Vetor de embedding da busca.</param>
        /// <param name="limit">Número máximo de documentos de contexto a retornar.</param>
        Task<string?> QueryCollectionAsync(string collectionId, double[] queryEmbedding, int limit = 5);
    }
}
