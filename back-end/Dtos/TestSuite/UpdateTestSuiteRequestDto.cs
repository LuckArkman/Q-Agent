using System;

namespace Dtos.TestSuite
{
    /// <summary>
    /// Contrato contendo os dados editáveis de uma suíte de testes.
    /// </summary>
    public class UpdateTestSuiteRequestDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? CronExpression { get; set; }
        public Guid AgentConfigId { get; set; }
    }
}
