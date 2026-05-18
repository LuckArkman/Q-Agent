using System;

namespace Dtos.Agent
{
    /// <summary>
    /// Contrato de dados que retorna informações de configuração do Agente de IA.
    /// </summary>
    public class AgentConfigResponseDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; } // WhatsApp, Telegram, Chatbot, etc.
        public required string Endpoint { get; set; }
        public required string ApiKey { get; set; } // Retornará mascarada por segurança em listagens gerais
        public DateTime CreatedAt { get; set; }
    }
}
