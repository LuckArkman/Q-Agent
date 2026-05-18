using System.Collections.Generic;
using Dtos.Simulation;

namespace Services.Evaluators
{
    /// <summary>
    /// Interface para cálculo de proximidade semântica utilizando a métrica Best Matching 25 (BM25).
    /// </summary>
    public interface IBm25Evaluator
    {
        /// <summary>
        /// Calcula a proximidade semântica normalizada entre a resposta do agente e as fatias obtidas do RAG.
        /// </summary>
        /// <param name="agentResponse">Resposta textual gerada pelo agente.</param>
        /// <param name="contextChunks">Trechos e fatias de RAG utilizados como base de conhecimento.</param>
        /// <returns>Nota de proximidade semântica normalizada entre 0.0 e 1.0.</returns>
        double CalculateBm25Score(string agentResponse, List<RagContextChunkDto> contextChunks);
    }
}
