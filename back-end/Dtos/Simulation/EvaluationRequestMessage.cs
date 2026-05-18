using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dtos.Simulation
{
    /// <summary>
    /// Mensagem que trafega na fila in-memory carregando a requisição de avaliação semântica assíncrona.
    /// </summary>
    public class EvaluationRequestMessage
    {
        public string Prompt { get; set; } = string.Empty;
        public string ChatbotResponse { get; set; } = string.Empty;
        public List<RagContextChunkDto> ContextChunks { get; set; } = new List<RagContextChunkDto>();

        /// <summary>
        /// Canal de callback thread-safe para retornar os laudos processados de forma assíncrona ao publicador original.
        /// </summary>
        public TaskCompletionSource<EvaluationReportDto> CompletionSource { get; } = new TaskCompletionSource<EvaluationReportDto>();
    }

    /// <summary>
    /// Laudo consolidado contendo as 4 métricas calculadas pelo LLM Judge.
    /// </summary>
    public class EvaluationReportDto
    {
        public double FaithfulnessScore { get; set; }
        public bool IsFaithful { get; set; }
        public List<string> FaithfulnessReasoning { get; set; } = new List<string>();

        public double RelevanceScore { get; set; }
        public bool IsRelevant { get; set; }
        public List<string> RelevanceReasoning { get; set; } = new List<string>();

        public double ContextPrecisionScore { get; set; }
        public bool IsPrecise { get; set; }
        public List<string> ContextPrecisionReasoning { get; set; } = new List<string>();

        public double ContextRecallScore { get; set; }
        public bool IsRecalled { get; set; }
        public List<string> ContextRecallReasoning { get; set; } = new List<string>();

        /// <summary>
        /// Média aritmética ponderada ou global de todas as notas semânticas calculadas.
        /// </summary>
        public double GlobalAuditScore { get; set; }

        /// <summary>
        /// Status global de conformidade da simulação (aprovado se todas as métricas passarem de 0.8).
        /// </summary>
        public bool IsApproved { get; set; }
    }
}
