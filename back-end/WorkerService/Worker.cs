using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dtos.Simulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Evaluators;
using Services.Queue;

namespace WorkerService
{
    /// <summary>
    /// Hosted background service encarregado de consumir as solicitações de simulação, disparar as avaliações semânticas em paralelo e preencher os relatórios de laudo técnico.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ITestExecutionQueue _testExecutionQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(
            ITestExecutionQueue testExecutionQueue,
            IServiceProvider serviceProvider,
            ILogger<Worker> logger)
        {
            _testExecutionQueue = testExecutionQueue ?? throw new ArgumentNullException(nameof(testExecutionQueue));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Ciclo de vida principal do Hosted Service. Escuta a fila assíncrona in-memory continuamente.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker Service de Simulação de IA iniciado e ouvindo a Queue Engine...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Aguarda de forma não-bloqueante a chegada de uma nova simulação para auditoria
                    var request = await _testExecutionQueue.DequeueEvaluationAsync(stoppingToken);

                    // Dispara a avaliação em segundo plano (ThreadPool) para não engarrafar o canal de leitura
                    _ = Task.Run(async () =>
                    {
                        await ProcessEvaluationRequestAsync(request);
                    }, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Ouvinte da fila interrompido por sinalização de encerramento do sistema.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro de processamento na escuta do canal de simulações.");
                }
            }
        }

        /// <summary>
        /// Orquestra a execução concorrente dos 4 avaliadores de IA usando um escopo isolado DI e aguardando com Task.WhenAll.
        /// </summary>
        private async Task ProcessEvaluationRequestAsync(EvaluationRequestMessage message)
        {
            _logger.LogInformation("Iniciando auditoria semântica para a pergunta: '{Prompt}'", message.Prompt);

            // BOAS PRÁTICAS: Singleton para Scoped DI resolve por meio de IServiceScope isolado
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var faithfulnessEvaluator = scope.ServiceProvider.GetRequiredService<IFaithfulnessEvaluator>();
                var answerRelevanceEvaluator = scope.ServiceProvider.GetRequiredService<IAnswerRelevanceEvaluator>();
                var contextPrecisionEvaluator = scope.ServiceProvider.GetRequiredService<IContextPrecisionEvaluator>();
                var contextRecallEvaluator = scope.ServiceProvider.GetRequiredService<IContextRecallEvaluator>();

                // EXCELÊNCIA TÉCNICA: Dispara as 4 consultas ao LLM Judge em paralelo para ganho massivo de performance
                var faithfulnessTask = faithfulnessEvaluator.EvaluateFaithfulnessAsync(message.Prompt, message.ChatbotResponse, message.ContextChunks);
                var relevanceTask = answerRelevanceEvaluator.EvaluateAnswerRelevanceAsync(message.Prompt, message.ChatbotResponse);
                var precisionTask = contextPrecisionEvaluator.EvaluateContextPrecisionAsync(message.Prompt, message.ContextChunks);
                var recallTask = contextRecallEvaluator.EvaluateContextRecallAsync(message.Prompt, message.ChatbotResponse, message.ContextChunks);

                await Task.WhenAll(faithfulnessTask, relevanceTask, precisionTask, recallTask);

                var faithfulnessResult = await faithfulnessTask;
                var relevanceResult = await relevanceTask;
                var precisionResult = await precisionTask;
                var recallResult = await recallTask;

                // Consolida as notas semânticas calculadas
                var globalScore = (faithfulnessResult.Score + relevanceResult.Score + precisionResult.Score + recallResult.Score) / 4.0;
                var isApproved = faithfulnessResult.IsFaithful && 
                                 relevanceResult.IsRelevant && 
                                 precisionResult.IsPrecise && 
                                 recallResult.IsRecalled;

                var report = new EvaluationReportDto
                {
                    FaithfulnessScore = faithfulnessResult.Score,
                    IsFaithful = faithfulnessResult.IsFaithful,
                    FaithfulnessReasoning = faithfulnessResult.Reasoning,

                    RelevanceScore = relevanceResult.Score,
                    IsRelevant = relevanceResult.IsRelevant,
                    RelevanceReasoning = relevanceResult.Reasoning,

                    ContextPrecisionScore = precisionResult.Score,
                    IsPrecise = precisionResult.IsPrecise,
                    ContextPrecisionReasoning = precisionResult.Reasoning,

                    ContextRecallScore = recallResult.Score,
                    IsRecalled = recallResult.IsRecalled,
                    ContextRecallReasoning = recallResult.Reasoning,

                    GlobalAuditScore = globalScore,
                    IsApproved = isApproved
                };

                _logger.LogInformation("Auditoria semântica finalizada com sucesso! Score Global: {GlobalScore:F2} - Status: {Status}", globalScore, isApproved ? "APROVADO" : "REJEITADO");

                // Retorna o resultado de volta ao publicador original por meio da TCS
                message.CompletionSource.TrySetResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha crítica ao executar a auditoria em paralelo para: '{Prompt}'", message.Prompt);
                
                // Em caso de desastre, injeta um relatório com as métricas de falha sem travar o worker
                var failReport = new EvaluationReportDto
                {
                    FaithfulnessScore = 0.0,
                    RelevanceScore = 0.0,
                    ContextPrecisionScore = 0.0,
                    ContextRecallScore = 0.0,
                    GlobalAuditScore = 0.0,
                    IsApproved = false
                };
                message.CompletionSource.TrySetResult(failReport);
            }
        }
    }
}