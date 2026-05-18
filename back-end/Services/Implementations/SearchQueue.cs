using System.Threading.Channels;
using System.Threading.Tasks;
using Dtos.Simulation;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe concreta que implementa o canal de comunicação in-memory baseado em BoundedChannel de alta vazão e baixo overhead de GC.
    /// </summary>
    public class SearchQueue : ISearchQueue
    {
        private readonly Channel<SearchRequestMessage> _channel;

        public SearchQueue()
        {
            // BOAS PRÁTICAS: Canal bounded para evitar estouros de memória sob surtos de requisições de testes paralelos
            var options = new BoundedChannelOptions(2000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true, // Somente o hosted Worker lerá a fila
                SingleWriter = false // Múltiplos controllers e threads do Web API podem postar
            };
            _channel = Channel.CreateBounded<SearchRequestMessage>(options);
        }

        public ChannelReader<SearchRequestMessage> Reader => _channel.Reader;

        /// <summary>
        /// Enfileira assincronamente a mensagem na fila.
        /// </summary>
        public async ValueTask EnqueueAsync(SearchRequestMessage message)
        {
            await _channel.Writer.WriteAsync(message);
        }
    }
}
