using System;
using System.Threading;
using System.Threading.Tasks;
using Database.Chroma;
using Dtos.Simulation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Embeddings;
using Services.Interfaces;
using Services.Chroma;

namespace SearchService
{
    /// <summary>
    /// Hosted Background Service encarregado de escutar a fila in-memory de busca semântica, gerando vetores e consultando o ChromaDB de forma paralela.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ISearchQueue _searchQueue;
        private readonly IEmbeddingGenerator _embeddingGenerator;
        private readonly IChromaClient _chromaClient;
        private readonly ChromaSearchOptimizer _searchOptimizer;
        private readonly ILogger<Worker> _logger;

        public Worker(
            ISearchQueue searchQueue,
            IEmbeddingGenerator embeddingGenerator,
            IChromaClient chromaClient,
            ChromaSearchOptimizer searchOptimizer,
            ILogger<Worker> logger)
        {
            _searchQueue = searchQueue ?? throw new ArgumentNullException(nameof(searchQueue));
            _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
            _chromaClient = chromaClient ?? throw new ArgumentNullException(nameof(chromaClient));
            _searchOptimizer = searchOptimizer ?? throw new ArgumentNullException(nameof(searchOptimizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loop executor assíncrono que consome mensagens da fila in-memory com tratamento de shutdown gracioso.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Service de Busca Semântica inicializado com sucesso.");

            // BOAS PRÁTICAS: Escuta contínua e tratada pelo stoppingToken para shutdown limpo
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Aguarda novas mensagens estarem disponíveis sem travar threads de processamento
                    if (await _searchQueue.Reader.WaitToReadAsync(stoppingToken))
                    {
                        while (_searchQueue.Reader.TryRead(out var requestMessage))
                        {
                            // Dispara processamento em segundo plano de forma paralela para otimização de latência
                            _ = ProcessRequestAsync(requestMessage, stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Shutdown gracioso ativado
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no loop consumidor do worker de busca semântica.");
                    try
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            _logger.LogInformation("Background Service de Busca Semântica finalizado com sucesso.");
        }

        /// <summary>
        /// Processa uma única busca semântica de forma isolada, impedindo falhas de rede de travar o canal.
        /// </summary>
        private async Task ProcessRequestAsync(SearchRequestMessage request, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Processando busca vetorial. Prompt: {Prompt}", request.PromptText);

                // 1. Traduzir o prompt para float[] usando o gerador de embeddings otimizado
                var floatVector = await _embeddingGenerator.GenerateEmbeddingAsync(request.PromptText);

                // 2. Converter float[] para double[] conforme esperado pela interface IChromaClient
                var doubleVector = new double[floatVector.Length];
                for (int i = 0; i < floatVector.Length; i++)
                {
                    doubleVector[i] = floatVector[i];
                }

                // 3. Realizar consulta de similaridade no banco vetorial ChromaDB com tolerância a falhas Polly
                var searchResult = await _searchOptimizer.ExecuteAsync(
                    async token => await _chromaClient.QueryCollectionAsync(request.CollectionId, doubleVector, request.Limit),
                    stoppingToken);

                // 4. Concluir o callback devolvendo o payload bruto ao solicitante original
                request.ResponseCompletion.TrySetResult(searchResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha crítica ao processar busca semântica assíncrona.");
                request.ResponseCompletion.TrySetException(ex);
            }
        }
    }
}