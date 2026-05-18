using System.Threading.Tasks;
using Dtos.Simulation;

namespace Services.Evaluators
{
    /// <summary>
    /// Interface para o avaliador de relevância conversacional (identificador de respostas genéricas, evasivas ou off-topic).
    /// </summary>
    public interface IAnswerRelevanceEvaluator
    {
        /// <summary>
        /// Compara a resposta gerada pelo chatbot diretamente contra a pergunta original do usuário para julgar sua pertinência e precisão.
        /// </summary>
        /// <param name="prompt">A pergunta original feita pelo usuário.</param>
        /// <param name="response">A resposta gerada pelo chatbot.</param>
        /// <param name="contextDescription">Descrição de contexto opcional para auditoria adicional.</param>
        Task<AnswerRelevanceResultDto> EvaluateAnswerRelevanceAsync(string prompt, string response, string? contextDescription = null);
    }
}
