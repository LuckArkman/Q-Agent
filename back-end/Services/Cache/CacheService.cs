using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Services.Cache
{
    /// <summary>
    /// Implementação robusta de cache distribuído híbrido utilizando IDistributedCache,
    /// com suporte a serialização System.Text.Json e invalidação em lote por prefixo.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;
        
        // Mantém um registro thread-safe das chaves inseridas para viabilizar a invalidação híbrida por prefixo (Memory/Redis Fallback)
        private static readonly ConcurrentDictionary<string, byte> _trackedKeys = new ConcurrentDictionary<string, byte>();

        public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return default;

            try
            {
                var cachedData = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogDebug("[Cache CacheMiss] Chave: '{Key}'", key);
                    return default;
                }

                _logger.LogDebug("[Cache CacheHit] Chave: '{Key}'", key);
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao ler chave de cache '{Key}'. Retornando padrão (fail-safe).", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null) return;

            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var serialized = JsonSerializer.Serialize(value, jsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // Padrão: 5 minutos
                }

                await _distributedCache.SetStringAsync(key, serialized, options);
                _trackedKeys.TryAdd(key, 0); // Registra a chave localmente para controle de prefixo

                _logger.LogDebug("[Cache Set] Gravado chave: '{Key}' | Expiração: {Exp} minutos", key, options.AbsoluteExpirationRelativeToNow?.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gravar cache para a chave '{Key}'. Ignorando falha para resiliência.", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                await _distributedCache.RemoveAsync(key);
                _trackedKeys.TryRemove(key, out _);
                _logger.LogDebug("[Cache Remove] Chave removida: '{Key}'", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover chave '{Key}' do cache.", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return;

            _logger.LogInformation("[Cache Invalidation] Iniciando invalidação em lote por prefixo: '{Prefix}*'", prefix);

            try
            {
                // Filtra as chaves rastreadas que batem com o prefixo
                var keysToRemove = _trackedKeys.Keys
                    .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogInformation("[Cache Invalidation] Invalidação concluída. Chaves removidas: {Count}", keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha durante a invalidação de chaves por prefixo '{Prefix}'.", prefix);
            }
        }
    }
}
