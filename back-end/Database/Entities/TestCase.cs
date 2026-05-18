using System;
using System.Collections.Generic;

namespace Database.Entities
{
    /// <summary>
    /// Representa um caso de teste individual contendo o prompt do usuário e a resposta esperada de referência.
    /// </summary>
    public class TestCase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TestSuiteId { get; set; }
        public required string UserPrompt { get; set; }
        public string? ExpectedAnswer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public TestSuite? TestSuite { get; set; }
    }
}
