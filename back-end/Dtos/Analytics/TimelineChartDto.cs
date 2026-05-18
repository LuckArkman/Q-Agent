using System;
using System.Collections.Generic;

namespace Dtos.Analytics
{
    /// <summary>
    /// DTO contendo a série temporal de médias de qualidade para alimentar gráficos lineares e de área no front-end.
    /// </summary>
    public class TimelineChartDto
    {
        /// <summary>
        /// A lista cronológica ordenada de pontos de dados diários.
        /// </summary>
        public List<TimelinePointDto> Points { get; set; } = new List<TimelinePointDto>();
    }

    /// <summary>
    /// Representa a consolidação média das métricas de auditoria para um dia específico.
    /// </summary>
    public class TimelinePointDto
    {
        /// <summary>
        /// O rótulo do dia formatado (Ex: "yyyy-MM-dd").
        /// </summary>
        public required string DateLabel { get; set; }

        /// <summary>
        /// O total de simulações executadas neste dia específico.
        /// </summary>
        public long ExecutionCount { get; set; }

        /// <summary>
        /// A média do score de qualidade geral do dia.
        /// </summary>
        public double AverageGeneralQuality { get; set; }

        /// <summary>
        /// A média do score de Fidelidade do dia.
        /// </summary>
        public double AverageFaithfulness { get; set; }

        /// <summary>
        /// A média do score de Relevância de Resposta do dia.
        /// </summary>
        public double AverageAnswerRelevance { get; set; }

        /// <summary>
        /// A média do score de Precisão do Contexto do dia.
        /// </summary>
        public double AverageContextPrecision { get; set; }

        /// <summary>
        /// A média do score de Cobertura do Contexto do dia.
        /// </summary>
        public double AverageContextRecall { get; set; }
    }
}
