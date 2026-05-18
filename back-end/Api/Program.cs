using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Database.Context;
using Database.Chroma;
using Database.Mongo;
using Repositorys.Interfaces;
using Repositorys.Implementations;
using Services.Interfaces;
using Services.Implementations;
using Services.Adapters;
using Services.Embeddings;
using Services.RAG;
using Services.Chroma;
using Services.Evaluators;
using Services.Queue;
using Serilog;
using Api.Middlewares;

// Configuração inicial do Serilog para logging estruturado e rastreabilidade
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/qagent-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Iniciando o host da API Web do Q-Agent...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Configuração do DbContext PostgreSQL
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configuração do MongoDB Context (Singleton é ideal para MongoClient)
    builder.Services.AddSingleton<MongoContext>();

    // CACHE RESILIENTE: Configura a infraestrutura de Caching Híbrido (Memory/Redis)
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSingleton<Services.Cache.ICacheService, Services.Cache.CacheService>();

    // Registro dos Repositórios no DI
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
    builder.Services.AddScoped<IAgentConfigRepository, AgentConfigRepository>();
    builder.Services.AddScoped<ITestSuiteRepository, TestSuiteRepository>();
    builder.Services.AddScoped<ITestCaseRepository, TestCaseRepository>();
    builder.Services.AddScoped<IEvaluationHistoryRepository, EvaluationHistoryRepository>();

    // Registro dos Serviços no DI
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAgentConfigService, AgentConfigService>();
    builder.Services.AddScoped<ITestSuiteService, TestSuiteService>();
    builder.Services.AddScoped<ITestCaseService, TestCaseService>();
    builder.Services.AddScoped<IChatbotSimulatorService, ChatbotSimulatorService>();
    builder.Services.AddSingleton<IEmbeddingGenerator, EmbeddingGenerator>();
    builder.Services.AddScoped<IChromaIndexingService, ChromaIndexingService>();
    builder.Services.AddSingleton<ISearchQueue, SearchQueue>();
    builder.Services.AddScoped<IRagRetrievalService, RagRetrievalService>();
    builder.Services.AddSingleton<ChromaSearchOptimizer>();
    builder.Services.AddScoped<IFaithfulnessEvaluator, FaithfulnessEvaluator>();
    builder.Services.AddScoped<IAnswerRelevanceEvaluator, AnswerRelevanceEvaluator>();
    builder.Services.AddScoped<IContextPrecisionEvaluator, ContextPrecisionEvaluator>();
    builder.Services.AddScoped<IContextRecallEvaluator, ContextRecallEvaluator>();
    builder.Services.AddScoped<IBm25Evaluator, Bm25Evaluator>();
    builder.Services.AddSingleton<ITestExecutionQueue, TestExecutionQueue>();
    builder.Services.AddScoped<ITestOrchestratorService, TestOrchestratorService>();
    builder.Services.AddScoped<IChatbotAdapter, WhatsAppChatbotAdapter>();
    builder.Services.AddScoped<IChatbotAdapter, TelegramChatbotAdapter>();
    builder.Services.AddScoped<IChatbotAdapter, CustomApiChatbotAdapter>();

    // RESILIÊNCIA E CIRCUITO HTTP: Configuração do HttpClient padrão com as políticas Polly
    builder.Services.AddHttpClient(string.Empty)
        .AddPolicyHandler(Services.Chroma.PollyPolicies.GetRetryPolicy())
        .AddPolicyHandler(Services.Chroma.PollyPolicies.GetCircuitBreakerPolicy());

    // Registro do ChromaDB Client com as Políticas de Resiliência do Polly
    builder.Services.AddHttpClient<IChromaClient, ChromaClient>()
        .AddPolicyHandler(Services.Chroma.PollyPolicies.GetRetryPolicy())
        .AddPolicyHandler(Services.Chroma.PollyPolicies.GetCircuitBreakerPolicy());

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // MONITORAMENTO DE SAÚDE: Configura verificações ativas de integridade física contra as bases de dados
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
        .AddMongoDb(builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017", name: "mongodb")
        .AddCheck<Api.Health.ChromaHealthCheck>("chromadb");

    var app = builder.Build();

    // RASTREABILIDADE: Registra o CorrelationIdMiddleware no início do pipeline HTTP
    app.UseMiddleware<CorrelationIdMiddleware>();

    // TRATAMENTO GLOBAL: Registra o ExceptionHandlingMiddleware para capturar erros e formatar respostas sob RFC 7807
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseAuthorization();

    // MONITORAMENTO DE SAÚDE: Mapeia o endpoint /health para verificação física simplificada (Healthy/Unhealthy)
    app.MapHealthChecks("/health");

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O host da API Web foi encerrado de forma inesperada durante a inicialização.");
}
finally
{
    Log.CloseAndFlush();
}