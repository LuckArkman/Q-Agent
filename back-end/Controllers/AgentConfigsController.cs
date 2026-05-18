using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dtos.Agent;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar as rotas HTTP de CRUD e parametrizações cadastrais dos agentes de IA.
    /// </summary>
    [ApiController]
    [Route("api/agents")]
    public class AgentConfigsController : ControllerBase
    {
        private readonly IAgentConfigService _agentService;
        private readonly ILogger<AgentConfigsController> _logger;

        public AgentConfigsController(IAgentConfigService agentService, ILogger<AgentConfigsController> logger)
        {
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lista todos os agentes de IA parametrizados no sistema, aplicando máscara de proteção de chaves por padrão.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AgentConfigResponseDto>>> GetAll([FromQuery] bool mask = true)
        {
            _logger.LogInformation("Solicitação de listagem geral de agentes de IA (Máscara de chaves: {Mask}).", mask);

            try
            {
                var list = await _agentService.GetAllAsync(mask);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar lista de agentes.");
                return StatusCode(500, new { Message = "Erro crítico ao listar agentes." });
            }
        }

        /// <summary>
        /// Recupera as configurações de um agente específico através de seu Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AgentConfigResponseDto>> GetById(Guid id, [FromQuery] bool mask = true)
        {
            _logger.LogInformation("Solicitação de detalhamento do agente: '{Id}' (Máscara: {Mask}).", id, mask);

            try
            {
                var agent = await _agentService.GetByIdAsync(id, mask);
                if (agent == null)
                {
                    _logger.LogWarning("Agente '{Id}' não localizado no banco relacional.", id);
                    return NotFound(new { Message = $"Agente com ID '{id}' não encontrado." });
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao detalhar agente '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico ao detalhar agente." });
            }
        }

        /// <summary>
        /// Cadastra um novo agente de IA e executa testes físicos de conectividade antes de autorizar a persistência.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AgentConfigResponseDto>> Create([FromBody] CreateAgentConfigRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Solicitação de cadastro de novo agente: '{Name}' | Endpoint: '{Endpoint}'", request.Name, request.Endpoint);

            try
            {
                var createdAgent = await _agentService.CreateAsync(request);
                if (createdAgent == null)
                {
                    _logger.LogWarning("Falha no cadastro do agente '{Name}': Conectividade HTTP ou DNS com o endpoint falhou.", request.Name);
                    return BadRequest(new { Message = "Não foi possível cadastrar o agente. O endpoint fornecido está inacessível ou inválido." });
                }

                _logger.LogInformation("Agente '{Name}' cadastrado com sucesso sob ID: '{Id}'.", request.Name, createdAgent.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdAgent.Id }, createdAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante o cadastro do agente '{Name}'.", request.Name);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar cadastrar agente." });
            }
        }

        /// <summary>
        /// Atualiza os parâmetros de cadastro de um agente de IA existente.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AgentConfigResponseDto>> Update(Guid id, [FromBody] UpdateAgentConfigRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Solicitação de atualização para o agente: '{Id}'", id);

            try
            {
                var updatedAgent = await _agentService.UpdateAsync(id, request);
                if (updatedAgent == null)
                {
                    _logger.LogWarning("Falha ao atualizar agente '{Id}': registro não encontrado ou endpoint inválido.", id);
                    return NotFound(new { Message = $"Agente com ID '{id}' não localizado para atualização ou novos parâmetros de conectividade inválidos." });
                }

                _logger.LogInformation("Agente '{Id}' atualizado com sucesso.", id);
                return Ok(updatedAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a atualização do agente '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar atualizar agente." });
            }
        }

        /// <summary>
        /// Exclui um agente de IA do banco de dados relacional.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Solicitação de exclusão do agente: '{Id}'", id);

            try
            {
                var deleted = await _agentService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Falha ao deletar agente '{Id}': registro não localizado.", id);
                    return NotFound(new { Message = $"Agente com ID '{id}' não encontrado para exclusão." });
                }

                _logger.LogInformation("Agente '{Id}' removido com sucesso do sistema.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a exclusão do agente '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar remover agente." });
            }
        }

        /// <summary>
        /// Endpoint auxiliar para testar manualmente a conectividade DNS e HTTP contra uma URL sem salvar alterações.
        /// </summary>
        [HttpPost("validate-connectivity")]
        public async Task<IActionResult> ValidateConnectivity([FromQuery] string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return BadRequest(new { Message = "O parâmetro 'endpoint' é obrigatório." });
            }

            _logger.LogInformation("Testando conectividade de endpoint isolado: '{Endpoint}'", endpoint);

            try
            {
                var isOnline = await _agentService.ValidateEndpointConnectivityAsync(endpoint);
                return Ok(new { Endpoint = endpoint, IsOnline = isOnline });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao validar conectividade do endpoint '{Endpoint}'.", endpoint);
                return StatusCode(500, new { Message = "Erro ao testar conectividade com o servidor alvo." });
            }
        }
    }
}
