using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Evaluators
{
    /// <summary>
    /// Interface para o avaliador de recall de contexto (Context Recall), encarregado de classificar se o chatbot cobriu todas as diretrizes cruciais fornecidas pelas fatias de conhecimento.
    /// </summary>
    public interface IContextRecallEvaluator
    {
        /// <summary>
        /// Compara a resposta gerada contra cada bloco individual de RAG recuperado, avaliando se fatos fundamentais foram esquecidos ou se sinônimos distorceram as regras.
        /// </summary>
        /// <param name="prompt">A pergunta original feita pelo usuário.</param>
        /// <param name="response">A resposta formulada pelo chatbot sob simulação.</param>
        /// <param name="contextChunks">A lista ordenada de fatias de contexto RAG recuperadas pelo ChromaDB.</param>
        Task<ContextRecallResultDto> EvaluateContextRecallAsync(string prompt, string response, List<RagContextChunkDto> contextChunks);
    }
}
