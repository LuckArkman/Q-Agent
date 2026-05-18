using System.Collections.Generic;

namespace Dtos.Simulation
{
    /// <summary>
    /// Representa o resultado estruturado da avaliação de completude e recall de contexto (Context Recall) do LLM Judge.
    /// </summary>
    public class ContextRecallResultDto
    {
        /// <summary>
        /// Nota de recall de contexto variando de 0.0 (esqueceu todos os pontos críticos do contexto) a 1.0 (citou e cobriu 100% dos fatos cruciais).
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Justificativa detalhada analisando quais regras ou fatos cruciais do contexto de referência foram omitidos ou incluídos.
        /// </summary>
        public List<string> Reasoning { get; set; } = new List<string>();

        /// <summary>
        /// Sinaliza se as fatias de RAG possuem completude/recall adequado (Score >= 0.8).
        /// </summary>
        public bool IsRecalled { get; set; }

        /// <summary>
        /// O retorno textual/JSON bruto enviado pela IA juíza para fins de logs e auditoria interna.
        /// </summary>
        public string RawLlmResponse { get; set; } = string.Empty;
    }
}
