using System.Threading.Tasks;

namespace Services.Embeddings
{
    /// <summary>
    /// Contrato para geração de representações vetoriais de alta dimensão (Embeddings) a partir de blocos de textos.
    /// </summary>
    public interface IEmbeddingGenerator
    {
        /// <summary>
        /// Gera uma representação vetorial (float[]) a partir de um bloco de texto, com otimização de cache integrada.
        /// </summary>
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
