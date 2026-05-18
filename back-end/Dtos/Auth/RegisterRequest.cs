namespace Dtos.Auth
{
    /// <summary>
    /// Contrato de dados enviado para realizar o cadastro de um novo usuário.
    /// </summary>
    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? FullName { get; set; }
    }
}
