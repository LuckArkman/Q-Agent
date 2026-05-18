using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Evaluators
{
    /// <summary>
    /// Interface para o avaliador de fidelidade conversacional (identificador de alucinações baseadas em RAG).
    /// </summary>
    public interface IFaithfulnessEvaluator
    {
        /// <summary>
        /// Compara a resposta do chatbot contra as fatias de conhecimento recuperadas e calcula o nível de fidelidade.
        /// </summary>
        /// <param name="prompt">O prompt original enviado pelo usuário.</param>
        /// <param name="response">A resposta gerada pelo chatbot sob simulação.</param>
        /// <param name="contextChunks">A lista de documentos recuperados do ChromaDB que dão suporte.</param>
        Task<FaithfulnessResultDto> EvaluateFaithfulnessAsync(string prompt, string response, List<RagContextChunkDto> contextChunks);
    }
}
