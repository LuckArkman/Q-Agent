using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Evaluators
{
    /// <summary>
    /// Interface para o avaliador de precisão e ranqueamento de contextos (Context Precision), responsável por classificar se as buscas vetoriais retornaram apenas dados úteis.
    /// </summary>
    public interface IContextPrecisionEvaluator
    {
        /// <summary>
        /// Compara a pergunta formulada diretamente contra cada bloco individual de RAG recuperado, avaliando sua relevância relativa e ordenação ideal.
        /// </summary>
        /// <param name="prompt">A pergunta original feita pelo usuário.</param>
        /// <param name="contextChunks">A lista ordenada de fatias de contexto trazida pelo ChromaDB.</param>
        Task<ContextPrecisionResultDto> EvaluateContextPrecisionAsync(string prompt, List<RagContextChunkDto> contextChunks);
    }
}
