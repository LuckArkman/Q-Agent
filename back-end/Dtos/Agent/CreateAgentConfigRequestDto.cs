namespace Dtos.Agent
{
    /// <summary>
    /// Contrato contendo os dados necessários para cadastrar um novo chatbot.
    /// </summary>
    public class CreateAgentConfigRequestDto
    {
        public required string Name { get; set; }
        public required string Type { get; set; } // Ex: "WhatsApp", "Telegram", "WebChat"
        public required string Endpoint { get; set; } // URL do bot (Ex: http://localhost:5000/webhook)
        public required string ApiKey { get; set; } // Credencial do bot
    }
}
