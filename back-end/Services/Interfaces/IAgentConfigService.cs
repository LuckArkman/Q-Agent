using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.Agent;

namespace Services.Interfaces
{
    /// <summary>
    /// Define as regras de negócio para gerenciar o ciclo de vida dos agentes de IA, incluindo validações de rede e URLs.
    /// </summary>
    public interface IAgentConfigService
    {
        /// <summary>
        /// Cadastra um novo agente de IA, validando a acessibilidade física de sua rota HTTP.
        /// </summary>
        Task<AgentConfigResponseDto?> CreateAsync(CreateAgentConfigRequestDto request);

        /// <summary>
        /// Edita as configurações cadastrais de um agente, preservando segredos caso nenhuma nova chave seja informada.
        /// </summary>
        Task<AgentConfigResponseDto?> UpdateAsync(Guid id, UpdateAgentConfigRequestDto request);

        /// <summary>
        /// Obtém os dados cadastrais de um agente por Id, opcionalmente aplicando máscara de segurança sobre a ApiKey.
        /// </summary>
        Task<AgentConfigResponseDto?> GetByIdAsync(Guid id, bool maskKey = true);

        /// <summary>
        /// Lista todas as configurações dos chatbots ativos no sistema.
        /// </summary>
        Task<IEnumerable<AgentConfigResponseDto>> GetAllAsync(bool maskKeys = true);

        /// <summary>
        /// Exclui um agente de IA do sistema.
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Testa a conectividade física e DNS contra o endpoint alvo do chatbot com timeout controlado.
        /// </summary>
        Task<bool> ValidateEndpointConnectivityAsync(string endpoint);
    }
}
