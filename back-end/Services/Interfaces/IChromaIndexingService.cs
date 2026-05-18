using System.Threading.Tasks;

namespace Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço encarregado de fatiar documentos de contexto e indexá-los no banco vetorial ChromaDB.
    /// </summary>
    public interface IChromaIndexingService
    {
        /// <summary>
        /// Fatia um texto de conhecimento respeitando limites de blocos e sobreposições, gera seus embeddings e os cataloga no ChromaDB.
        /// </summary>
        /// <param name="collectionName">Nome da coleção destino.</param>
        /// <param name="documentId">Identificador único de rastreabilidade do documento.</param>
        /// <param name="text">O texto de conteúdo integral.</param>
        /// <param name="chunkSize">O tamanho máximo em caracteres de cada bloco de texto (chunk).</param>
        /// <param name="overlap">A sobreposição de caracteres entre blocos consecutivos (overlap).</param>
        Task<bool> IndexDocumentAsync(string collectionName, string documentId, string text, int chunkSize = 500, int overlap = 50);
    }
}
