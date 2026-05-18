using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dtos.Simulation;
using Services.Interfaces;
using Services.Queue;
using Services.RAG;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    /// <summary>
    /// Implementação de excelência técnica para o orquestrador geral de simulações.
    /// Despacha simultaneamente consultas vetoriais ao ChromaDB e handshakes de rede com os adaptadores antes de enfileirar a auditoria.
    /// </summary>
    public class TestOrchestratorService : ITestOrchestratorService
    {
        private readonly IChatbotSimulatorService _chatbotSimulatorService;
        private readonly IRagRetrievalService _ragRetrievalService;
        private readonly ITestExecutionQueue _testExecutionQueue;
        private readonly ILogger<TestOrchestratorService> _logger;

        public TestOrchestratorService(
            IChatbotSimulatorService chatbotSimulatorService,
            IRagRetrievalService ragRetrievalService,
            ITestExecutionQueue testExecutionQueue,
            ILogger<TestOrchestratorService> logger)
        {
            _chatbotSimulatorService = chatbotSimulatorService ?? throw new ArgumentNullException(nameof(chatbotSimulatorService));
            _ragRetrievalService = ragRetrievalService ?? throw new ArgumentNullException(nameof(ragRetrievalService));
            _testExecutionQueue = testExecutionQueue ?? throw new ArgumentNullException(nameof(testExecutionQueue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Orquestra a execução concorrente de busca vetorial e chatbot webhook, publicando em seguida na fila assíncrona do LLM Judge.
        /// </summary>
        public async Task<EvaluationReportDto> OrchestrateTestStepAsync(
            string prompt,
            string agentConfigId,
            string collectionName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Orquestrando nova etapa de teste. Agente: '{AgentId}' | Coleção Chroma: '{Collection}'", agentConfigId, collectionName);

            try
            {
                // Converte a string ID do agente para Guid correspondente à assinatura do serviço de simulação
                var agentGuid = Guid.Parse(agentConfigId);

                // EXCELÊNCIA TÉCNICA: Dispara as buscas de RAG e a simulação física do webhook de forma 100% concorrente
                var chatbotTask = _chatbotSimulatorService.SimulateMessageAsync(agentGuid, prompt);
                var ragTask = _ragRetrievalService.RetrieveRelevantContextAsync(collectionName, prompt);

                // Aguarda a finalização simultânea de ambos os fluxos
                await Task.WhenAll(chatbotTask, ragTask);

                var chatbotResponse = await chatbotTask;
                var contextChunks = await ragTask;

                // Valida a resposta física do chatbot
                if (chatbotResponse == null || !chatbotResponse.IsSuccess)
                {
                    var errorMsg = chatbotResponse?.ErrorMessage ?? "Falha de conexão desconhecida com o chatbot.";
                    _logger.LogError("O adaptador do chatbot retornou uma falha de conexão física: {Error}", errorMsg);

                    return new EvaluationReportDto
                    {
                        GlobalAuditScore = 0.0,
                        IsApproved = false,
                        FaithfulnessReasoning = new List<string> { $"Simulação abortada: {errorMsg}" },
                        RelevanceReasoning = new List<string> { "Não foi possível auditar a relevância devido à queda física do chatbot." },
                        ContextPrecisionReasoning = new List<string> { $"Contextos RAG resgatados com sucesso ({contextChunks.Count} chunks), mas inutilizados." }
                    };
                }

                _logger.LogInformation("Webhook do chatbot respondeu com sucesso (Latência: {Latency}ms). Enviando para a fila de avaliação semântica...", chatbotResponse.LatencyMs);

                // Monta a mensagem que trafegará na fila BoundedChannel do Worker
                var evaluationRequest = new EvaluationRequestMessage
                {
                    Prompt = prompt,
                    ChatbotResponse = chatbotResponse.ResponseText ?? string.Empty,
                    ContextChunks = contextChunks
                };

                // Publica na fila em memória assíncrona
                await _testExecutionQueue.EnqueueEvaluationAsync(evaluationRequest, cancellationToken);

                // Aguarda de forma não-bloqueante até que o Worker processe as 4 notas e devolva o parecer técnico consolidado
                var technicalReport = await evaluationRequest.CompletionSource.Task;

                _logger.LogInformation("Laudo técnico recebido do LLM Judge para a pergunta: '{Prompt}' | Score Global: {Score}", prompt, technicalReport.GlobalAuditScore);

                return technicalReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de orquestração ao tentar rodar a etapa de teste.");
                return new EvaluationReportDto
                {
                    GlobalAuditScore = 0.0,
                    IsApproved = false,
                    FaithfulnessReasoning = new List<string> { $"Falha interna da engine do orquestrador: {ex.Message}" }
                };
            }
        }
    }
}
