using System;

namespace Dtos.TestSuite
{
    /// <summary>
    /// Contrato de dados que retorna detalhes seguros de uma suíte de testes.
    /// </summary>
    public class TestSuiteResponseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? CronExpression { get; set; } // Agendamento em formato Cron
        public Guid AgentConfigId { get; set; } // Agente de IA associado
        public DateTime CreatedAt { get; set; }
        public int TestCasesCount { get; set; } // Contador leve de cenários associados
    }
}
