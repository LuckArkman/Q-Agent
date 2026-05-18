namespace Dtos.TestCase
{
    /// <summary>
    /// Contrato contendo os dados editáveis de um cenário de teste.
    /// </summary>
    public class UpdateTestCaseRequestDto
    {
        public required string InputPrompt { get; set; }
        public required string ExpectedOutput { get; set; }
    }
}
