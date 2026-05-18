namespace Dtos.User
{
    /// <summary>
    /// Contrato de dados contendo dados editáveis do perfil do usuário.
    /// </summary>
    public class UpdateProfileRequestDto
    {
        public string? FullName { get; set; }
        public required string Email { get; set; }
    }
}
