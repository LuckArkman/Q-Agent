using System;

namespace Dtos.TestSuite
{
    /// <summary>
    /// Contrato contendo os dados necessários para cadastrar uma nova suíte de testes.
    /// </summary>
    public class CreateTestSuiteRequestDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? CronExpression { get; set; } // Agendamento recorrente opcional
        public Guid AgentConfigId { get; set; } // Identificador do bot monitorado
    }
}
