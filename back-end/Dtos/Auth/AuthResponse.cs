namespace Dtos.Auth
{
    /// <summary>
    /// Contrato de dados retornado após uma autenticação ou registro bem-sucedido.
    /// </summary>
    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
