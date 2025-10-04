using Microsoft.EntityFrameworkCore;
using EXAPARCIALALVARO.Models;
using EXAPARCIALALVARO.Data;

namespace EXAPARCIALALVARO.Services
{
    public class CursosService : ICursosService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCacheService _cache;
        private readonly ILogger<CursosService> _logger;

        private const string CACHE_KEY_CURSOS_ACTIVOS = "cursos_activos";
        private readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromSeconds(60); // 60 segundos

        public CursosService(ApplicationDbContext context, IMemoryCacheService cache, ILogger<CursosService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<Curso>> GetCursosActivosAsync()
        {
            // Intentar obtener del cache primero
            var cachedCursos = await _cache.GetAsync<List<Curso>>(CACHE_KEY_CURSOS_ACTIVOS);
            if (cachedCursos != null && cachedCursos.Any())
            {
                _logger.LogInformation("🎯 Cursos obtenidos desde CACHE - {Count} cursos", cachedCursos.Count);
                return cachedCursos;
            }

            // Si no está en cache, obtener de la base de datos
            _logger.LogInformation("🗃️ Cursos obtenidos desde BASE DE DATOS");
            var cursos = await _context.Cursos
                .Where(c => c.Activo)
                .Include(c => c.Matriculas)
                .ToListAsync();

            // Guardar en cache para próximas peticiones
            await _cache.SetAsync(CACHE_KEY_CURSOS_ACTIVOS, cursos, CACHE_EXPIRATION);

            return cursos;
        }

        public async Task InvalidarCacheCursosAsync()
        {
            await _cache.RemoveAsync(CACHE_KEY_CURSOS_ACTIVOS);
            _logger.LogInformation("🔄 Cache de cursos INVALIDADO");
        }
    }
}