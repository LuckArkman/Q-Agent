using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Database.Chroma;
using Services.Interfaces;
using Services.Implementations;
using Services.Embeddings;
using Services.Chroma;
using SearchService;

var builder = Host.CreateApplicationBuilder(args);

// Registro do canal in-memory (Message Broker leve)
builder.Services.AddSingleton<ISearchQueue, SearchQueue>();

// Registro do gerador de embeddings vetoriais (Singleton)
builder.Services.AddSingleton<IEmbeddingGenerator, EmbeddingGenerator>();

// Registro do cliente ChromaDB usando HttpClient Typed Factory
builder.Services.AddHttpClient<IChromaClient, ChromaClient>();

// Registro do otimizador de buscas e disjuntor de tolerância a falhas (Singleton)
builder.Services.AddSingleton<ChromaSearchOptimizer>();

// Registro do Hosted Background Service (Consumidor)
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();