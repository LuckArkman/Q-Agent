using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Queue
{
    /// <summary>
    /// Implementação robusta baseada em System.Threading.Channels com limite de memória estrito (Bounded Capacity)
    /// para gerenciar concorrência de escrita de múltiplos hosts API e consumo em segundo plano.
    /// </summary>
    public class TestExecutionQueue : ITestExecutionQueue
    {
        private readonly Channel<EvaluationRequestMessage> _channel;

        public TestExecutionQueue()
        {
            var options = new BoundedChannelOptions(5000) // Limite de 5000 requisições simultâneas para prevenir estouro de memória RAM
            {
                FullMode = BoundedChannelFullMode.Wait,  // Faz com que os publicadores aguardem caso a fila esteja sobrecarregada
                SingleReader = true,                    // Otimiza para um único leitor (Worker de simulação)
                SingleWriter = false                    // Permite múltiplas threads gravarem (Múltiplas instâncias ou requisições de API)
            };

            _channel = Channel.CreateBounded<EvaluationRequestMessage>(options);
        }

        /// <summary>
        /// Publica de forma não bloqueante ou aguarda espaço caso o canal esteja saturado.
        /// </summary>
        public async ValueTask EnqueueEvaluationAsync(EvaluationRequestMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            await _channel.Writer.WriteAsync(message, cancellationToken);
        }

        /// <summary>
        /// Bloqueia e aguarda de forma assíncrona até que um novo item de teste esteja pronto para consumo.
        /// </summary>
        public async ValueTask<EvaluationRequestMessage> DequeueEvaluationAsync(CancellationToken cancellationToken = default)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
