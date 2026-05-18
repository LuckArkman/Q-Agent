namespace Dtos.Auth
{
    /// <summary>
    /// Contrato de dados enviado para realizar o login do usuário.
    /// </summary>
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
