using System;

namespace Dtos.TestCase
{
    /// <summary>
    /// Contrato contendo os dados necessários para cadastrar um novo prompt/cenário de teste.
    /// </summary>
    public class CreateTestCaseRequestDto
    {
        public Guid TestSuiteId { get; set; }
        public required string InputPrompt { get; set; }
        public required string ExpectedOutput { get; set; }
    }
}
