using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Database.Entities;
using Repositorys.Interfaces;
using Dtos.Agent;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe que implementa o ciclo de vida dos agentes de IA, aplicando validação HTTP ativa e mascaramento de segredos.
    /// </summary>
    public class AgentConfigService : IAgentConfigService
    {
        private readonly IAgentConfigRepository _agentRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public AgentConfigService(IAgentConfigRepository agentRepository, IHttpClientFactory httpClientFactory)
        {
            _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<AgentConfigResponseDto?> CreateAsync(CreateAgentConfigRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validar formato básico do Endpoint
            if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out _))
                throw new ArgumentException("O endpoint informado não é uma URL absoluta válida.");

            // Testar conectividade ativa contra o chatbot (advisory/validação de sanidade)
            var isReachable = await ValidateEndpointConnectivityAsync(request.Endpoint);
            if (!isReachable)
            {
                // Em ambiente de produção, impedir o cadastro de rotas inativas é excelente para qualidade do pipeline de RAG.
                // Lançamos um aviso descritivo indicando a falha de handshake inicial.
                throw new ArgumentException("Não foi possível conectar ao endpoint do chatbot. Verifique se a URL está online e acessível.");
            }

            var agent = new AgentConfig
            {
                Name = request.Name,
                ChannelType = request.Type,
                EndpointUrl = request.Endpoint,
                ApiKey = request.ApiKey
            };

            await _agentRepository.AddAsync(agent);
            var success = await _agentRepository.SaveChangesAsync();

            return success ? MapToDto(agent, maskKey: true) : null;
        }

        public async Task<AgentConfigResponseDto?> UpdateAsync(Guid id, UpdateAgentConfigRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out _))
                throw new ArgumentException("O endpoint informado não é uma URL absoluta válida.");

            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
                return null;

            // Testar conectividade ativa contra a nova URL
            var isReachable = await ValidateEndpointConnectivityAsync(request.Endpoint);
            if (!isReachable)
                throw new ArgumentException("Não foi possível estabelecer conexão com o novo endpoint informado.");

            agent.Name = request.Name;
            agent.ChannelType = request.Type;
            agent.EndpointUrl = request.Endpoint;

            // Atualiza a chave secreta apenas se uma nova chave explícita for enviada
            if (!string.IsNullOrWhiteSpace(request.ApiKey))
            {
                agent.ApiKey = request.ApiKey;
            }

            _agentRepository.Update(agent);
            var success = await _agentRepository.SaveChangesAsync();

            return success ? MapToDto(agent, maskKey: true) : null;
        }

        public async Task<AgentConfigResponseDto?> GetByIdAsync(Guid id, bool maskKey = true)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
                return null;

            return MapToDto(agent, maskKey);
        }

        public async Task<IEnumerable<AgentConfigResponseDto>> GetAllAsync(bool maskKeys = true)
        {
            var list = await _agentRepository.GetAllAsync();
            var dtos = new List<AgentConfigResponseDto>();

            foreach (var agent in list)
            {
                dtos.Add(MapToDto(agent, maskKeys));
            }

            return dtos;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
                return false;

            _agentRepository.Delete(agent);
            return await _agentRepository.SaveChangesAsync();
        }

        public async Task<bool> ValidateEndpointConnectivityAsync(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint) || !Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
                return false;

            try
            {
                // Obter um cliente HTTP gerenciado pelo pool do AddHttpClient do ASP.NET
                using var client = _httpClientFactory.CreateClient();
                
                // Configurar um timeout extremamente curto (3 segundos) para evitar travamento de threads
                client.Timeout = TimeSpan.FromSeconds(3);

                // Efetuar um handshake HTTP GET leve. Qualquer resposta válida do servidor HTTP destino
                // (incluindo status como 404, 401 ou 200) prova que o host DNS foi resolvido e a máquina física responde.
                var response = await client.GetAsync(uri);
                return true;
            }
            catch (Exception)
            {
                // Captura resiliênte de falhas de DNS, timeouts de socket e conexões recusadas
                return false;
            }
        }

        /// <summary>
        /// Mapeador auxiliar privado com lógica integrada de mascaramento de credenciais.
        /// </summary>
        private static AgentConfigResponseDto MapToDto(AgentConfig agent, bool maskKey)
        {
            return new AgentConfigResponseDto
            {
                Id = agent.Id,
                Name = agent.Name,
                Type = agent.ChannelType,
                Endpoint = agent.EndpointUrl,
                ApiKey = maskKey ? "********" : (agent.ApiKey ?? ""),
                CreatedAt = agent.CreatedAt
            };
        }
    }
}
