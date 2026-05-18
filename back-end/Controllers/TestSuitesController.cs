using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dtos.TestSuite;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar as rotas HTTP de CRUD e agendamentos Cron das suítes de testes de integração.
    /// </summary>
    [ApiController]
    [Route("api/test-suites")]
    public class TestSuitesController : ControllerBase
    {
        private readonly ITestSuiteService _suiteService;
        private readonly ILogger<TestSuitesController> _logger;

        public TestSuitesController(ITestSuiteService suiteService, ILogger<TestSuitesController> logger)
        {
            _suiteService = suiteService ?? throw new ArgumentNullException(nameof(suiteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lista todas as suítes de testes cadastradas no sistema.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSuiteResponseDto>>> GetAll()
        {
            _logger.LogInformation("Solicitação de listagem geral de suítes de testes.");

            try
            {
                var list = await _suiteService.GetAllAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao listar suítes de testes.");
                return StatusCode(500, new { Message = "Erro crítico de servidor ao listar suítes de testes." });
            }
        }

        /// <summary>
        /// Detalha os parâmetros cadastrais e estatísticas de uma suíte de testes por Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TestSuiteResponseDto>> GetById(Guid id)
        {
            _logger.LogInformation("Solicitação de detalhamento de suíte de testes: '{Id}'", id);

            try
            {
                var suite = await _suiteService.GetByIdAsync(id);
                if (suite == null)
                {
                    _logger.LogWarning("Suíte de testes '{Id}' não localizada.", id);
                    return NotFound(new { Message = $"Suíte de testes com ID '{id}' não encontrada." });
                }

                return Ok(suite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes da suíte '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico ao detalhar suíte de testes." });
            }
        }

        /// <summary>
        /// Cadastra uma nova suíte de testes de integração, validando as parametrizações Cron de execução em segundo plano.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TestSuiteResponseDto>> Create([FromBody] CreateTestSuiteRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Solicitação de cadastro de nova suíte: '{Description}' | AgentId: '{AgentId}'", request.Description, request.AgentConfigId);

            try
            {
                var createdSuite = await _suiteService.CreateAsync(request);
                if (createdSuite == null)
                {
                    _logger.LogWarning("Falha ao criar suíte '{Description}': Agente associado inválido ou formato Cron inválido.", request.Description);
                    return BadRequest(new { Message = "Não foi possível criar a suíte. Verifique se o agente de IA associado existe e se a expressão Cron é válida." });
                }

                _logger.LogInformation("Suíte de testes '{Description}' cadastrada sob ID: '{Id}'.", request.Description, createdSuite.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdSuite.Id }, createdSuite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante o cadastro da suíte '{Description}'.", request.Description);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar cadastrar suíte de testes." });
            }
        }

        /// <summary>
        /// Edita as parametrizações cadastrais e de agendamento Cron de uma suíte de testes existente.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TestSuiteResponseDto>> Update(Guid id, [FromBody] UpdateTestSuiteRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Solicitação de atualização para a suíte de testes: '{Id}'", id);

            try
            {
                var updatedSuite = await _suiteService.UpdateAsync(id, request);
                if (updatedSuite == null)
                {
                    _logger.LogWarning("Falha ao atualizar suíte '{Id}': registro não localizado ou expressão Cron inválida.", id);
                    return NotFound(new { Message = $"Suíte de testes com ID '{id}' não localizada para atualização ou parametrizações de Cron/Agente inválidas." });
                }

                _logger.LogInformation("Suíte de testes '{Id}' atualizada com sucesso.", id);
                return Ok(updatedSuite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a atualização da suíte '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar atualizar suíte de testes." });
            }
        }

        /// <summary>
        /// Remove uma suíte de testes do banco de dados relacional e propaga a exclusão de cenários associados em cascata.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Solicitação de exclusão da suíte de testes: '{Id}'", id);

            try
            {
                var deleted = await _suiteService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Falha ao remover suíte '{Id}': registro não encontrado.", id);
                    return NotFound(new { Message = $"Suíte de testes com ID '{id}' não encontrada para exclusão." });
                }

                _logger.LogInformation("Suíte de testes '{Id}' removida com sucesso (Exclusão cascata executada).", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a exclusão da suíte '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar remover suíte de testes." });
            }
        }

        /// <summary>
        /// Endpoint auxiliar para validar se uma expressão Cron de agendamento é sintaticamente aceita.
        /// </summary>
        [HttpPost("validate-cron")]
        public async Task<IActionResult> ValidateCron([FromQuery] string cron)
        {
            if (string.IsNullOrWhiteSpace(cron))
            {
                return BadRequest(new { Message = "O parâmetro 'cron' é obrigatório." });
            }

            try
            {
                var isValid = await _suiteService.ValidateCronExpressionAsync(cron);
                return Ok(new { Expression = cron, IsValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar a expressão Cron '{Cron}'.", cron);
                return StatusCode(500, new { Message = "Erro ao processar validação do agendamento." });
            }
        }
    }
}
