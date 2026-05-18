using System;

namespace Dtos.User
{
    /// <summary>
    /// Contrato de dados que retorna detalhes seguros do perfil do usuário.
    /// </summary>
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? FullName { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
