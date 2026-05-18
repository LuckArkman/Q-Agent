using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Adapters
{
    /// <summary>
    /// Contrato unificado para adaptadores de comunicação conversacional de IA.
    /// </summary>
    public interface IChatbotAdapter
    {
        /// <summary>
        /// O tipo de canal específico suportado pelo adaptador (Ex: "WhatsApp", "Telegram", "GenericChatbot").
        /// </summary>
        string ChannelType { get; }

        /// <summary>
        /// Envia um prompt simulado ao chatbot destino, monitorando estritamente a resposta e a latência de rede.
        /// </summary>
        Task<ChatbotResponseDto> SendMessageAsync(string endpoint, string apiKey, string message);
    }
}
