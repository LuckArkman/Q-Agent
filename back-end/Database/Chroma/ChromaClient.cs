using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Database.Chroma
{
    /// <summary>
    /// Cliente HTTP para interação com a API REST nativa do ChromaDB.
    /// </summary>
    public class ChromaClient : IChromaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChromaClient> _logger;

        public ChromaClient(HttpClient httpClient, IConfiguration configuration, ILogger<ChromaClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var baseUrl = configuration["ChromaDB:BaseUrl"] ?? "http://localhost:8000";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/heartbeat");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Conectividade com o ChromaDB estabelecida com sucesso.");
                    return true;
                }
                _logger.LogWarning("ChromaDB retornou status de erro: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conectividade ao tentar acessar o ChromaDB.");
                return false;
            }
        }

        public async Task<string?> CreateCollectionAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome da coleção não pode ser nulo ou vazio.", nameof(name));

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/v1/collections", new { name });
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Coleção '{Name}' criada com sucesso no ChromaDB.", name);
                    return content;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao criar coleção '{Name}'. Código: {StatusCode}. Resposta: {Error}", 
                    name, response.StatusCode, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na requisição de criação de coleção no ChromaDB.");
                return null;
            }
        }

        public async Task<string?> QueryCollectionAsync(string collectionId, double[] queryEmbedding, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
                throw new ArgumentException("O identificador da coleção não pode ser nulo ou vazio.", nameof(collectionId));
            if (queryEmbedding == null || queryEmbedding.Length == 0)
                throw new ArgumentException("O vetor de embedding não pode ser nulo ou vazio.", nameof(queryEmbedding));

            try
            {
                var payload = new
                {
                    query_embeddings = new[] { queryEmbedding },
                    n_results = limit
                };
                
                var response = await _httpClient.PostAsJsonAsync($"/api/v1/collections/{collectionId}/query", payload);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao consultar coleção '{CollectionId}'. Código: {StatusCode}. Resposta: {Error}", 
                    collectionId, response.StatusCode, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na requisição de consulta semântica ao ChromaDB.");
                return null;
            }
        }
    }
}
