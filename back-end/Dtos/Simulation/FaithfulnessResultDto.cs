using System.Collections.Generic;

namespace Dtos.Simulation
{
    /// <summary>
    /// Representa o resultado estruturado da avaliação de fidelidade (Faithfulness) do LLM Judge.
    /// </summary>
    public class FaithfulnessResultDto
    {
        /// <summary>
        /// Nota de fidelidade variando de 0.0 (alucinação total) a 1.0 (totalmente suportada por documentos).
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Lista detalhada de sentenças/fatos analisados e suas respectivas justificativas de auditoria.
        /// </summary>
        public List<string> Reasoning { get; set; } = new List<string>();

        /// <summary>
        /// Sinaliza se a resposta é considerada fiel/confiável (Score >= 0.8).
        /// </summary>
        public bool IsFaithful { get; set; }

        /// <summary>
        /// O retorno JSON ou texto bruto enviado pela IA juíza para fins de logs e auditoria interna.
        /// </summary>
        public string RawLlmResponse { get; set; } = string.Empty;
    }
}
