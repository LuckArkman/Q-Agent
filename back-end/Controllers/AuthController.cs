using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dtos.Auth;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar as rotas HTTP de autenticação, registro e segurança de acessos.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Cadastra um novo usuário no banco relacional PostgreSQL e retorna o JWT correspondente.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { Message = "Usuário, senha e email são obrigatórios." });
            }

            _logger.LogInformation("Solicitação de registro recebida para o usuário: '{Username}'", request.Username);

            try
            {
                var response = await _authService.RegisterAsync(request);
                if (response == null)
                {
                    _logger.LogWarning("Falha no cadastro: Usuário ou email '{Username}' já existente.", request.Username);
                    return BadRequest(new { Message = "Nome de usuário ou e-mail já cadastrado no sistema." });
                }

                _logger.LogInformation("Usuário '{Username}' registrado com sucesso.", request.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante o processamento do registro de usuário.");
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar realizar cadastro." });
            }
        }

        /// <summary>
        /// Valida as credenciais enviadas e retorna o JWT se autenticado com sucesso.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Message = "O corpo da requisição não pode estar vazio." });
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Message = "Nome de usuário e senha são obrigatórios." });
            }

            _logger.LogInformation("Solicitação de autenticação para o usuário: '{Username}'", request.Username);

            try
            {
                var response = await _authService.LoginAsync(request);
                if (response == null)
                {
                    _logger.LogWarning("Tentativa de acesso não autorizada para: '{Username}'", request.Username);
                    return Unauthorized(new { Message = "Credenciais inválidas. Usuário ou senha incorretos." });
                }

                _logger.LogInformation("Usuário '{Username}' autenticado com sucesso. Token JWT emitido.", request.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante a autenticação do usuário.");
                return StatusCode(500, new { Message = "Erro crítico de servidor ao tentar realizar login." });
            }
        }
    }
}
