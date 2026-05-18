using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Interfaces;
using Services.Evaluators;
using Services.Implementations;
using WorkerService;
using Services.Queue;

var builder = Host.CreateApplicationBuilder(args);

// Registro do cliente HTTP tipado do LLM Judge associado à interface ILlmJudgeClient
builder.Services.AddHttpClient<ILlmJudgeClient, LlmJudgeClient>();

// Registro do avaliador de fidelidade semântica RAG (Scoped)
builder.Services.AddScoped<IFaithfulnessEvaluator, FaithfulnessEvaluator>();

// Registro do avaliador de relevância de resposta (Scoped)
builder.Services.AddScoped<IAnswerRelevanceEvaluator, AnswerRelevanceEvaluator>();

// Registro do avaliador de precisão e ordenação de contexto RAG (Scoped)
builder.Services.AddScoped<IContextPrecisionEvaluator, ContextPrecisionEvaluator>();

// Registro do avaliador de completude e recall de contexto RAG (Scoped)
builder.Services.AddScoped<IContextRecallEvaluator, ContextRecallEvaluator>();

// Registro da fila concorrente de execuções (Singleton)
builder.Services.AddSingleton<ITestExecutionQueue, TestExecutionQueue>();

// Registro do orquestrador central de simulações e testes (Scoped)
builder.Services.AddScoped<ITestOrchestratorService, TestOrchestratorService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();