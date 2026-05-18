using System.Threading.Tasks;

namespace Dtos.Simulation
{
    /// <summary>
    /// Modelo de mensagem contendo os parâmetros de busca vetorial assíncrona e canal de retorno.
    /// </summary>
    public class SearchRequestMessage
    {
        public required string PromptText { get; set; }
        public required string CollectionId { get; set; }
        public int Limit { get; set; } = 5;

        /// <summary>
        /// Canal de callback thread-safe para retornar a resposta bruta do ChromaDB ao solicitante.
        /// </summary>
        public TaskCompletionSource<string?> ResponseCompletion { get; } = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
