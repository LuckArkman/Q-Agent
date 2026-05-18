using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Database.Mongo
{
    /// <summary>
    /// Documento armazenado no MongoDB contendo todo o histórico de execuções de testes e métricas de avaliações semânticas.
    /// </summary>
    public class EvaluationHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid AgentConfigId { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid TestSuiteId { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid TestCaseId { get; set; }

        public required string SentPrompt { get; set; }
        public required string ReceivedResponse { get; set; }
        
        public double LatencyMs { get; set; }
        public int StatusCode { get; set; }

        public double FaithfulnessScore { get; set; } // Score de Fidelidade: 0.0 a 1.0
        public double AnswerRelevanceScore { get; set; } // Score de Relevância: 0.0 a 1.0
        public double ContextPrecisionScore { get; set; } // Score de Precisão: 0.0 a 1.0
        public double ContextRecallScore { get; set; } // Score de Recall: 0.0 a 1.0
        public double GeneralQualityScore { get; set; } // Média Geral: 0.0 a 1.0

        public string? JudgmentReasoning { get; set; } // Justificativa do LLM Judge
        public string? ModelUsedForJudge { get; set; } // gpt-4o, llama3...

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}
