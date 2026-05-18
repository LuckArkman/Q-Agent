using System.Threading;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Queue
{
    /// <summary>
    /// Interface para a fila assíncrona concorrente (BoundedChannel) que gerencia as solicitações de avaliação do LLM Judge.
    /// </summary>
    public interface ITestExecutionQueue
    {
        /// <summary>
        /// Publica um pedido de avaliação na fila in-memory de forma assíncrona e segura para multithreading.
        /// </summary>
        ValueTask EnqueueEvaluationAsync(EvaluationRequestMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Escuta e extrai o próximo pedido de avaliação disponível na fila in-memory.
        /// </summary>
        ValueTask<EvaluationRequestMessage> DequeueEvaluationAsync(CancellationToken cancellationToken = default);
    }
}
