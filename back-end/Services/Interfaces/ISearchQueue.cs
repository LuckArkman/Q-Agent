using System.Threading.Channels;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Interfaces
{
    /// <summary>
    /// Abstração do canal in-memory (Message Broker leve) para processamento assíncrono de buscas semânticas.
    /// </summary>
    public interface ISearchQueue
    {
        /// <summary>
        /// Canal leitor para consumo das mensagens pelo background worker.
        /// </summary>
        ChannelReader<SearchRequestMessage> Reader { get; }

        /// <summary>
        /// Enfileira uma nova requisição de busca semântica para processamento assíncrono.
        /// </summary>
        ValueTask EnqueueAsync(SearchRequestMessage message);
    }
}
