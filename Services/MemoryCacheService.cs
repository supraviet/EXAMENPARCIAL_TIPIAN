using Microsoft.Extensions.Caching.Memory;

namespace EXAPARCIALALVARO.Services
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                {
                    _logger.LogInformation("✅ Cache HIT - Key: {Key}", key);
                    return Task.FromResult<T?>(cachedValue);
                }

                _logger.LogInformation("❌ Cache MISS - Key: {Key}", key);
                return Task.FromResult<T?>(default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error leyendo cache - Key: {Key}", key);
                return Task.FromResult<T?>(default);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration);

                _memoryCache.Set(key, value, cacheOptions);
                _logger.LogInformation("💾 Cache SET - Key: {Key}, Expira: {Expiration}", key, expiration);
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error escribiendo cache - Key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogInformation("🗑️ Cache REMOVED - Key: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removiendo cache - Key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _memoryCache.TryGetValue(key, out _);
                return Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verificando cache - Key: {Key}", key);
                return Task.FromResult(false);
            }
        }
    }
}