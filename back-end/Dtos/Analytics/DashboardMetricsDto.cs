using System;

namespace Dtos.Analytics
{
    /// <summary>
    /// DTO contendo os indicadores analíticos de alto nível para exibição nos cartões principais do dashboard.
    /// </summary>
    public class DashboardMetricsDto
    {
        /// <summary>
        /// O volume total de simulações registradas no MongoDB.
        /// </summary>
        public long TotalExecutions { get; set; }

        /// <summary>
        /// A média consolidada do score de Fidelidade (Faithfulness) [0.0 - 1.0].
        /// </summary>
        public double AverageFaithfulness { get; set; }

        /// <summary>
        /// A média consolidada do score de Relevância da Resposta (Answer Relevance) [0.0 - 1.0].
        /// </summary>
        public double AverageAnswerRelevance { get; set; }

        /// <summary>
        /// A média consolidada do score de Precisão do Contexto (Context Precision) [0.0 - 1.0].
        /// </summary>
        public double AverageContextPrecision { get; set; }

        /// <summary>
        /// A média consolidada do score de Cobertura do Contexto (Context Recall) [0.0 - 1.0].
        /// </summary>
        public double AverageContextRecall { get; set; }

        /// <summary>
        /// A média consolidada da Qualidade Geral calculada (General Quality) [0.0 - 1.0].
        /// </summary>
        public double AverageGeneralQuality { get; set; }

        /// <summary>
        /// A latência média de processamento fim a fim da simulação física em milissegundos.
        /// </summary>
        public double AverageLatencyMs { get; set; }

        /// <summary>
        /// A taxa percentual de requisições concluídas com sucesso físico (Status HTTP 200).
        /// </summary>
        public double SuccessRate { get; set; }
    }
}
