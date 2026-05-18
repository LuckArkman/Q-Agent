using System;

namespace Dtos.TestCase
{
    /// <summary>
    /// Contrato de dados que retorna detalhes seguros de um cenário de teste (TestCase).
    /// </summary>
    public class TestCaseResponseDto
    {
        public Guid Id { get; set; }
        public Guid TestSuiteId { get; set; } // Suíte associada
        public required string InputPrompt { get; set; } // O prompt enviado na simulação
        public required string ExpectedOutput { get; set; } // Resposta esperada ideal (RAG baseline)
        public DateTime CreatedAt { get; set; }
    }
}
