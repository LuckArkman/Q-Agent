namespace Dtos.Simulation
{
    /// <summary>
    /// Contrato contendo os metadados e resposta de uma mensagem simulada enviada a um chatbot.
    /// </summary>
    public class ChatbotResponseDto
    {
        public string? ResponseText { get; set; }
        public long LatencyMs { get; set; } // Latência precisa de handshake em milissegundos
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
