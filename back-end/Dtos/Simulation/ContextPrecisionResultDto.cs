using System.Collections.Generic;

namespace Dtos.Simulation
{
    /// <summary>
    /// Representa o resultado estruturado da avaliação de precisão e relevância de contexto (Context Precision) do LLM Judge.
    /// </summary>
    public class ContextPrecisionResultDto
    {
        /// <summary>
        /// Nota de precisão de contexto variando de 0.0 (ranqueamento de ruído absoluto) a 1.0 (ranqueamento perfeito contendo apenas trechos relevantes no topo).
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Argumentação passo a passo detalhando quais fatias RAG são úteis e quais são apenas ruído redundante.
        /// </summary>
        public List<string> Reasoning { get; set; } = new List<string>();

        /// <summary>
        /// Sinaliza se as fatias de RAG possuem ranqueamento preciso (Score >= 0.8).
        /// </summary>
        public bool IsPrecise { get; set; }

        /// <summary>
        /// O retorno textual/JSON bruto enviado pela IA juíza para fins de logs e auditoria interna.
        /// </summary>
        public string RawLlmResponse { get; set; } = string.Empty;
    }
}
