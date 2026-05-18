using System;
using System.Collections.Generic;

namespace Database.Entities
{
    /// <summary>
    /// Representa a configuração de um agente de atendimento de inteligência artificial (WhatsApp, Telegram, etc).
    /// </summary>
    public class AgentConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string ChannelType { get; set; } // WhatsApp, Telegram, ChatbotAPI
        public required string EndpointUrl { get; set; }
        public string? ApiKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TestSuite> TestSuites { get; set; } = new List<TestSuite>();
    }
}
