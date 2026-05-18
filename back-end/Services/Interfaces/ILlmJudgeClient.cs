using System.Threading.Tasks;

namespace Services.Interfaces
{
    /// <summary>
    /// Interface abstrata para o cliente do LLM Judge, desacoplando o domínio de avaliação das rotas de rede HTTP.
    /// </summary>
    public interface ILlmJudgeClient
    {
        /// <summary>
        /// Dispara um prompt estruturado de auditoria para o provedor de IA ativo e retorna a resposta textual crua.
        /// </summary>
        Task<string?> ExecuteEvaluationAsync(string systemPrompt, string userPrompt);
    }
}
