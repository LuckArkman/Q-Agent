using System.Threading;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Interfaces
{
    /// <summary>
    /// Orquestrador geral que coordena o fluxo integrado de teste semântico:
    /// Dispara simulações de chatbot, busca fatias de RAG em paralelo, publica na fila assíncrona do Worker e aguarda o laudo técnico do LLM Judge.
    /// </summary>
    public interface ITestOrchestratorService
    {
        /// <summary>
        /// Orquestra uma única etapa conversacional, executando buscas e simulações simultaneamente antes de acionar a auditoria.
        /// </summary>
        /// <param name="prompt">A pergunta do usuário a ser submetida.</param>
        /// <param name="agentConfigId">O identificador do agente sob simulação no PostgreSQL.</param>
        /// <param name="collectionName">O nome da coleção do ChromaDB contendo a base de conhecimento.</param>
        /// <param name="cancellationToken">O token de cancelamento da operação.</param>
        Task<EvaluationReportDto> OrchestrateTestStepAsync(
            string prompt,
            string agentConfigId,
            string collectionName,
            CancellationToken cancellationToken = default);
    }
}
