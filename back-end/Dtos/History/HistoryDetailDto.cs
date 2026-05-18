using System;

namespace Dtos.History
{
    /// <summary>
    /// DTO que encapsula todos os detalhes de auditoria semântica e latência estruturados a partir do documento MongoDB.
    /// </summary>
    public class HistoryDetailDto
    {
        public string? Id { get; set; }
        public Guid AgentConfigId { get; set; }
        public Guid TestSuiteId { get; set; }
        public Guid TestCaseId { get; set; }
        
        public required string SentPrompt { get; set; }
        public required string ReceivedResponse { get; set; }
        
        public double LatencyMs { get; set; }
        public int StatusCode { get; set; }

        public double FaithfulnessScore { get; set; }
        public double AnswerRelevanceScore { get; set; }
        public double ContextPrecisionScore { get; set; }
        public double ContextRecallScore { get; set; }
        public double GeneralQualityScore { get; set; }

        public string? JudgmentReasoning { get; set; }
        public string? ModelUsedForJudge { get; set; }
        
        public DateTime ExecutedAt { get; set; }
    }
}
