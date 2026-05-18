using System;

namespace Database.Entities
{
    /// <summary>
    /// Representa a conta de usuário armazenada na base de dados relacional PostgreSQL.
    /// </summary>
    public class UserAccount
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public string? FullName { get; set; }
        public string Role { get; set; } = "User"; // SuperAdmin, Admin, User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
