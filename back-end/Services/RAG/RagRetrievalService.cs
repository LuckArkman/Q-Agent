using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dtos.Simulation;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Services.RAG
{
    /// <summary>
    /// Classe concreta que implementa o despacho de buscas semânticas assíncronas e estruturação dos dados cruas retornados do ChromaDB.
    /// </summary>
    public class RagRetrievalService : IRagRetrievalService
    {
        private readonly ISearchQueue _searchQueue;
        private readonly ILogger<RagRetrievalService> _logger;

        public RagRetrievalService(ISearchQueue searchQueue, ILogger<RagRetrievalService> logger)
        {
            _searchQueue = searchQueue ?? throw new ArgumentNullException(nameof(searchQueue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Envia o prompt para a fila concorrente, aguarda o callback do background worker com timeout de segurança e filtra os dados.
        /// </summary>
        public async Task<List<RagContextChunkDto>> RetrieveRelevantContextAsync(string collectionName, string prompt, int limit = 5, double maxDistanceThreshold = 1.2)
        {
            var results = new List<RagContextChunkDto>();
            if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(collectionName))
            {
                return results;
            }

            try
            {
                // 1. Criar a mensagem de solicitação vetorial
                var request = new SearchRequestMessage
                {
                    PromptText = prompt,
                    CollectionId = collectionName,
                    Limit = limit
                };

                // 2. Publicar na fila in-memory thread-safe
                await _searchQueue.EnqueueAsync(request);

                // 3. BOAS PRÁTICAS: Aguardar a resposta com timeout preventivo para evitar travamento da thread em caso de falha física do banco
                var rawJsonTask = request.ResponseCompletion.Task;
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));

                var completedTask = await Task.WhenAny(rawJsonTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    _logger.LogWarning("Timeout excedido (15s) ao aguardar busca vetorial para o prompt '{Prompt}'.", prompt);
                    return results;
                }

                var rawJson = await rawJsonTask;
                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    _logger.LogWarning("Resposta vetorial crua retornada pelo ChromaDB está vazia.");
                    return results;
                }

                // 4. Executar o parsing estruturado das respostas bidimensionais do ChromaDB
                using var jsonDoc = JsonDocument.Parse(rawJson);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("ids", out var idsProp) && idsProp.GetArrayLength() > 0 &&
                    root.TryGetProperty("documents", out var docsProp) && docsProp.GetArrayLength() > 0 &&
                    root.TryGetProperty("distances", out var distsProp) && distsProp.GetArrayLength() > 0)
                {
                    var idsArray = idsProp[0];
                    var docsArray = docsProp[0];
                    var distsArray = distsProp[0];

                    JsonElement metadatasArray = default;
                    bool hasMetadatas = root.TryGetProperty("metadatas", out metadatasArray) && metadatasArray.GetArrayLength() > 0;

                    int count = idsArray.GetArrayLength();
                    for (int i = 0; i < count; i++)
                    {
                        var chunkId = idsArray[i].GetString() ?? string.Empty;
                        var content = docsArray[i].GetString() ?? string.Empty;
                        var distance = distsArray[i].GetDouble();

                        // BOAS PRÁTICAS: Filtragem de relevância semântica (corta distâncias absurdas)
                        if (distance > maxDistanceThreshold)
                        {
                            _logger.LogInformation("Chunk '{ChunkId}' ignorado por distância excessiva ({Dist} > {Threshold}).", 
                                chunkId, distance, maxDistanceThreshold);
                            continue;
                        }

                        var chunkDto = new RagContextChunkDto
                        {
                            ChunkId = chunkId,
                            Content = content,
                            Distance = distance
                        };

                        if (hasMetadatas && i < metadatasArray[0].GetArrayLength())
                        {
                            var metaObj = metadatasArray[0][i];
                            if (metaObj.ValueKind == JsonValueKind.Object)
                            {
                                if (metaObj.TryGetProperty("document_id", out var docIdProp))
                                {
                                    chunkDto.DocumentId = docIdProp.GetString() ?? string.Empty;
                                }
                                if (metaObj.TryGetProperty("source", out var srcProp))
                                {
                                    chunkDto.Source = srcProp.GetString() ?? string.Empty;
                                }
                            }
                        }

                        results.Add(chunkDto);
                    }
                }

                if (results.Count == 0)
                {
                    // BOAS PRÁTICAS: Notificar cenários com zero correspondências válidas
                    _logger.LogWarning("Alerta de falta de relevância prévia: Nenhuma fatia vetorial atendeu ao threshold de {Threshold}.", maxDistanceThreshold);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar o parsing e estruturação das fatias de RAG.");
            }

            return results;
        }
    }
}
