using System;
using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço centralizador que orquestra a resolução dinâmica de adaptadores e dispara simulações contra chatbots plugados.
    /// </summary>
    public interface IChatbotSimulatorService
    {
        /// <summary>
        /// Carrega os parâmetros de um chatbot, resolve o adaptador correspondente (WhatsApp, Telegram, ChatbotAPI) e envia o prompt de simulação.
        /// </summary>
        Task<ChatbotResponseDto> SimulateMessageAsync(Guid agentConfigId, string message);
    }
}
