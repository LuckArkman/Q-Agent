namespace Dtos.Agent
{
    /// <summary>
    /// Contrato contendo os dados editáveis do agente de IA.
    /// </summary>
    public class UpdateAgentConfigRequestDto
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Endpoint { get; set; }
        public string? ApiKey { get; set; } // Caso seja nulo, mantém a chave anterior gravada no banco
    }
}
