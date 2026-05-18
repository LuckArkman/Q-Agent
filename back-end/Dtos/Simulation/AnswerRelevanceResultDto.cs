using System.Collections.Generic;

namespace Dtos.Simulation
{
    /// <summary>
    /// Representa o resultado estruturado da avaliação de relevância de resposta (Answer Relevance) do LLM Judge.
    /// </summary>
    public class AnswerRelevanceResultDto
    {
        /// <summary>
        /// Nota de relevância variando de 0.0 (totalmente evasivo/off-topic) a 1.0 (responde diretamente e com clareza a dúvida).
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Explicação passo a passo da IA juíza justificando a pontuação.
        /// </summary>
        public List<string> Reasoning { get; set; } = new List<string>();

        /// <summary>
        /// Sinaliza se a resposta é considerada relevante/útil (Score >= 0.8).
        /// </summary>
        public bool IsRelevant { get; set; }

        /// <summary>
        /// O retorno JSON ou texto bruto enviado pela IA juíza para fins de logs e auditoria interna.
        /// </summary>
        public string RawLlmResponse { get; set; } = string.Empty;
    }
}
