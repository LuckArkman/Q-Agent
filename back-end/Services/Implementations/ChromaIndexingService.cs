using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Database.Chroma;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Embeddings;
using Services.Interfaces;

namespace Services.Implementations
{
    /// <summary>
    /// Classe concreta que implementa o fatiamento de textos (chunking) e indexação de metadados no banco vetorial ChromaDB.
    /// </summary>
    public class ChromaIndexingService : IChromaIndexingService
    {
        private readonly IChromaClient _chromaClient;
        private readonly IEmbeddingGenerator _embeddingGenerator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChromaIndexingService> _logger;

        public ChromaIndexingService(
            IChromaClient chromaClient,
            IEmbeddingGenerator embeddingGenerator,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ChromaIndexingService> logger)
        {
            _chromaClient = chromaClient ?? throw new ArgumentNullException(nameof(chromaClient));
            _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fatia o documento base com overlap semântico e persiste os segmentos vetoriais no ChromaDB.
        /// </summary>
        public async Task<bool> IndexDocumentAsync(string collectionName, string documentId, string text, int chunkSize = 500, int overlap = 50)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("O nome da coleção destino não pode ser nulo ou vazio.", nameof(collectionName));
            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("O ID de rastreabilidade do documento não pode ser nulo ou vazio.", nameof(documentId));
            if (string.IsNullOrWhiteSpace(text))
                return false;

            try
            {
                using var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["ChromaDB:BaseUrl"] ?? "http://localhost:8000";
                client.BaseAddress = new Uri(baseUrl);

                // 1. Obter ou criar a coleção no ChromaDB para ler o UUID correspondente
                var collectionId = await GetOrCreateCollectionIdAsync(client, collectionName);
                if (string.IsNullOrWhiteSpace(collectionId))
                {
                    _logger.LogError("Não foi possível criar ou resolver a coleção '{Name}' no ChromaDB.", collectionName);
                    return false;
                }

                // BOAS PRÁTICAS: Fatiar o texto respeitando os limites de blocos e overlap para coerência semântica
                var textChunks = ChunkText(text, chunkSize, overlap);
                if (textChunks.Count == 0)
                {
                    _logger.LogWarning("O fatiador retornou zero blocos de texto para o documento '{DocId}'.", documentId);
                    return false;
                }

                var ids = new List<string>();
                var embeddings = new List<float[]>();
                var metadatas = new List<object>();
                var documents = new List<string>();

                for (int i = 0; i < textChunks.Count; i++)
                {
                    var chunkText = textChunks[i];
                    var chunkId = $"{documentId}_chunk_{i}";

                    // Gerar o embedding direcional para a fatia correspondente
                    var vector = await _embeddingGenerator.GenerateEmbeddingAsync(chunkText);

                    ids.Add(chunkId);
                    embeddings.Add(vector);
                    documents.Add(chunkText);
                    metadatas.Add(new
                    {
                        document_id = documentId,
                        chunk_index = i,
                        source = "Q-Agent Semantic Indexer"
                    });
                }

                // Payload nativo esperado pelo endpoint de adição do ChromaDB (/collections/{id}/add)
                var payload = new
                {
                    ids = ids,
                    embeddings = embeddings,
                    metadatas = metadatas,
                    documents = documents
                };

                var response = await client.PostAsJsonAsync($"/api/v1/collections/{collectionId}/add", payload);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Indexação bem-sucedida! {Count} chunks do documento '{DocId}' persistidos na coleção '{CollName}'.", 
                        textChunks.Count, documentId, collectionName);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Falha de REST no ChromaDB ao indexar chunks. Código: {Code}. Resposta: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro físico inesperado durante a indexação de chunks de RAG.");
                return false;
            }
        }

        /// <summary>
        /// Algoritmo resiliente para obter o UUID da coleção no ChromaDB.
        /// Se a coleção já existir, realiza um GET para recuperar o ID.
        /// </summary>
        private async Task<string?> GetOrCreateCollectionIdAsync(HttpClient client, string collectionName)
        {
            // Tenta criar a coleção inicialmente
            var createResult = await _chromaClient.CreateCollectionAsync(collectionName);
            if (!string.IsNullOrWhiteSpace(createResult))
            {
                try
                {
                    using var doc = JsonDocument.Parse(createResult);
                    if (doc.RootElement.TryGetProperty("id", out var idProp))
                    {
                        return idProp.GetString();
                    }
                }
                catch
                {
                    // Se o parsing falhar, prossegue para tentar recuperar via GET
                }
            }

            // Caso a criação retorne nulo (coleção já existente), recupera os metadados dela
            try
            {
                var response = await client.GetAsync($"/api/v1/collections/{collectionName}");
                if (response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(raw);
                    if (doc.RootElement.TryGetProperty("id", out var idProp))
                    {
                        return idProp.GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar buscar a coleção '{Name}' no ChromaDB via API REST.", collectionName);
            }

            return null;
        }

        /// <summary>
        /// Fatiador de texto com overlap para sobreposição semântica estruturada.
        /// </summary>
        private static List<string> ChunkText(string text, int chunkSize, int overlap)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) 
                return chunks;

            if (chunkSize <= 0) chunkSize = 500;
            if (overlap < 0 || overlap >= chunkSize) overlap = chunkSize / 10; // Overlap de segurança de 10%

            int index = 0;
            while (index < text.Length)
            {
                int length = Math.Min(chunkSize, text.Length - index);
                chunks.Add(text.Substring(index, length));

                if (index + length >= text.Length)
                    break;

                index += chunkSize - overlap;
            }

            return chunks;
        }
    }
}
