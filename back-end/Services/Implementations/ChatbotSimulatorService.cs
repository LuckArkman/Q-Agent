using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositorys.Interfaces;
using Dtos.Simulation;
using Services.Adapters;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe concreta que atua como orquestradora central das simulações, resolvendo polimorficamente o adaptador do canal configurado.
    /// </summary>
    public class ChatbotSimulatorService : IChatbotSimulatorService
    {
        private readonly IAgentConfigRepository _agentRepository;
        private readonly IEnumerable<IChatbotAdapter> _adapters;

        public ChatbotSimulatorService(IAgentConfigRepository agentRepository, IEnumerable<IChatbotAdapter> adapters)
        {
            _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
            _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));
        }

        /// <summary>
        /// Localiza o chatbot, determina o adaptador correspondente no Strategy Pool e dispara o envio simulado.
        /// </summary>
        public async Task<ChatbotResponseDto> SimulateMessageAsync(Guid agentConfigId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return new ChatbotResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "A mensagem do prompt de simulação não pode estar vazia ou nula."
                };
            }

            try
            {
                // Carregar as configurações de conexão e credenciais físicas do bot no Postgres
                var agent = await _agentRepository.GetByIdAsync(agentConfigId);
                if (agent == null)
                {
                    return new ChatbotResponseDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"O agente de IA com identificador '{agentConfigId}' não foi localizado no banco relacional."
                    };
                }

                if (string.IsNullOrWhiteSpace(agent.EndpointUrl))
                {
                    return new ChatbotResponseDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"O chatbot '{agent.Name}' não possui uma URL/Endpoint de webhook cadastrado."
                    };
                }

                // FACTORY / STRATEGY PATTERN: Localizar no Strategy Pool o adaptador adequado para o canal do agente (Ex: WhatsApp, Telegram, ChatbotAPI)
                var targetAdapter = _adapters.FirstOrDefault(a => 
                    a.ChannelType.Equals(agent.ChannelType, StringComparison.OrdinalIgnoreCase));

                if (targetAdapter == null)
                {
                    return new ChatbotResponseDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Não há suporte configurado para canal do tipo '{agent.ChannelType}'. Registros aceitos: 'WhatsApp', 'Telegram', 'ChatbotAPI'."
                    };
                }

                // Encaminhar o processamento e retornar o cálculo e a latência de rede isolados
                return await targetAdapter.SendMessageAsync(agent.EndpointUrl, agent.ApiKey ?? string.Empty, message);
            }
            catch (Exception ex)
            {
                // BOAS PRÁTICAS: Capturar erros genéricos de rede e formatação e registrar sem interromper outras threads do pool superior
                return new ChatbotResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"Falha crítica inesperada durante a simulação da mensagem: {ex.Message}"
                };
            }
        }
    }
}
