using System.Threading.Tasks;
using Dtos.Auth;

namespace Services.Interfaces
{
    /// <summary>
    /// Define as regras de negócio essenciais para segurança, criptografia e login de contas.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Realiza o cadastro de uma nova conta de usuário, cifrando a senha com BCrypt.
        /// </summary>
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Autentica o usuário, validando o hash da senha e emitindo o token JWT correspondente.
        /// </summary>
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }
}
