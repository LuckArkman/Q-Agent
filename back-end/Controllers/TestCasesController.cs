using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Dtos.TestCase;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar cenários individuais de testes de auditoria e importação massiva de prompts via CSV.
    /// </summary>
    [ApiController]
    [Route("api/test-cases")]
    public class TestCasesController : ControllerBase
    {
        private readonly ITestCaseService _testCaseService;
        private readonly ILogger<TestCasesController> _logger;

        public TestCasesController(ITestCaseService testCaseService, ILogger<TestCasesController> logger)
        {
            _testCaseService = testCaseService ?? throw new ArgumentNullException(nameof(testCaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Lista todos os cenários de testes de uma suíte específica.
        /// </summary>
        [HttpGet("suite/{testSuiteId:guid}")]
        public async Task<ActionResult<IEnumerable<TestCaseResponseDto>>> GetBySuiteId(Guid testSuiteId)
        {
            _logger.LogInformation("Solicitação de cenários para a suíte: '{SuiteId}'", testSuiteId);

            try
            {
                var list = await _testCaseService.GetByTestSuiteIdAsync(testSuiteId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar cenários para suíte '{SuiteId}'.", testSuiteId);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao listar cenários de testes." });
            }
        }

        /// <summary>
        /// Detalha um cenário de teste específico por seu Id.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TestCaseResponseDto>> GetById(Guid id)
        {
            _logger.LogInformation("Solicitação de detalhamento do cenário: '{Id}'", id);

            try
            {
                var testCase = await _testCaseService.GetByIdAsync(id);
                if (testCase == null)
                {
                    _logger.LogWarning("Cenário de teste '{Id}' não localizado.", id);
                    return NotFound(new { Message = $"Cenário de teste com ID '{id}' não encontrado." });
                }

                return Ok(testCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao detalhar cenário '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico ao detalhar cenário de teste." });
            }
        }

        /// <summary>
        /// Cadastra um cenário de teste individual associado a uma suíte.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TestCaseResponseDto>> Create([FromBody] CreateTestCaseRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Solicitação de cadastro de novo cenário na suíte: '{SuiteId}'", request.TestSuiteId);

            try
            {
                var createdCase = await _testCaseService.CreateAsync(request);
                if (createdCase == null)
                {
                    _logger.LogWarning("Falha ao criar cenário: Suíte de testes '{SuiteId}' não existe.", request.TestSuiteId);
                    return BadRequest(new { Message = "Não foi possível criar o cenário de teste. A suíte de testes associada é inválida." });
                }

                _logger.LogInformation("Cenário de teste cadastrado com sucesso sob ID: '{Id}'.", createdCase.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdCase.Id }, createdCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante o cadastro do cenário de teste.");
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar cadastrar cenário de teste." });
            }
        }

        /// <summary>
        /// Atualiza os parâmetros e prompts de um cenário de teste existente.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TestCaseResponseDto>> Update(Guid id, [FromBody] UpdateTestCaseRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Solicitação de atualização para o cenário: '{Id}'", id);

            try
            {
                var updatedCase = await _testCaseService.UpdateAsync(id, request);
                if (updatedCase == null)
                {
                    _logger.LogWarning("Falha ao atualizar cenário '{Id}': registro não localizado ou parâmetros inválidos.", id);
                    return NotFound(new { Message = $"Cenário de teste com ID '{id}' não localizado para atualização." });
                }

                _logger.LogInformation("Cenário de teste '{Id}' updated com sucesso.", id);
                return Ok(updatedCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a atualização do cenário '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar atualizar cenário de teste." });
            }
        }

        /// <summary>
        /// Exclui um cenário de teste individual do banco relacional.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Solicitação de exclusão do cenário: '{Id}'", id);

            try
            {
                var deleted = await _testCaseService.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Falha ao remover cenário '{Id}': registro não localizado.", id);
                    return NotFound(new { Message = $"Cenário de teste com ID '{id}' não encontrado para exclusão." });
                }

                _logger.LogInformation("Cenário de teste '{Id}' removido com sucesso do banco de dados.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a exclusão do cenário '{Id}'.", id);
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar remover cenário de teste." });
            }
        }

        /// <summary>
        /// Endpoint para importação em massa de prompts via upload de arquivo CSV.
        /// Valida rigorosamente extensões e tipos MIME para mitigar injeções e ataques.
        /// </summary>
        [HttpPost("import/{testSuiteId:guid}")]
        public async Task<IActionResult> ImportCsv(Guid testSuiteId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Nenhum arquivo CSV foi enviado." });
            }

            // SEGURANÇA ATIVA: Validação de extensão rígida contra injeção de arquivos maliciosos
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".csv")
            {
                return BadRequest(new { Message = "Extensão de arquivo inválida. Apenas arquivos do tipo '.csv' são autorizados." });
            }

            // SEGURANÇA ATIVA: Validação de tipo MIME para prevenir spoofing e execução arbitrária
            var mimeType = file.ContentType.ToLowerInvariant();
            if (mimeType != "text/csv" && mimeType != "application/vnd.ms-excel" && mimeType != "application/octet-stream")
            {
                return BadRequest(new { Message = "Tipo de mídia MIME inválido. Envie um arquivo CSV legítimo." });
            }

            _logger.LogInformation("Solicitação de importação em massa para a suíte '{SuiteId}'. Arquivo: '{FileName}' | Tamanho: {Length} bytes", testSuiteId, file.FileName, file.Length);

            try
            {
                using var stream = file.OpenReadStream();
                var importResult = await _testCaseService.ImportCsvAsync(testSuiteId, stream);

                _logger.LogInformation("Importação em massa finalizada! Sucessos: {SuccessCount} | Falhas: {ErrorCount}", importResult.SuccessCount, importResult.ErrorCount);
                return Ok(importResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de processamento durante a importação do arquivo CSV.");
                return StatusCode(500, new { Message = "Erro crítico interno de servidor ao tentar importar prompts." });
            }
        }
    }
}
