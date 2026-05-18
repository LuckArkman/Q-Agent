using System;
using System.Collections.Generic;

namespace Database.Entities
{
    /// <summary>
    /// Representa uma suíte de testes que agrupa múltiplos cenários de teste para um agente de IA.
    /// </summary>
    public class TestSuite
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AgentConfigId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CronSchedule { get; set; } // Opcional para agendamento automático
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AgentConfig? AgentConfig { get; set; }
        public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}
