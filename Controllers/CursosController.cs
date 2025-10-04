using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAPARCIALALVARO.Models;
using EXAPARCIALALVARO.Data;
using EXAPARCIALALVARO.Services;

namespace EXAPARCIALALVARO.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CursosController> _logger;
        private readonly ICursosService _cursosService;

        public CursosController(
            ApplicationDbContext context,
            ILogger<CursosController> logger,
            ICursosService cursosService)
        {
            _context = context;
            _logger = logger;
            _cursosService = cursosService;
        }

        // GET: Cursos - Catálogo con filtros (USANDO CACHE)
        public async Task<IActionResult> Index(CatalogoViewModel model)
        {
            model.Filtros ??= new FiltrosCursos();

            // Obtener cursos desde cache o base de datos
            var cursos = await _cursosService.GetCursosActivosAsync();

            // Aplicar filtros en memoria
            if (!string.IsNullOrEmpty(model.Filtros.Nombre))
            {
                cursos = cursos.Where(c =>
                    c.Nombre.Contains(model.Filtros.Nombre, StringComparison.OrdinalIgnoreCase) ||
                    c.Codigo.Contains(model.Filtros.Nombre, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (model.Filtros.CreditosMin.HasValue)
            {
                cursos = cursos.Where(c => c.Creditos >= model.Filtros.CreditosMin.Value).ToList();
            }

            if (model.Filtros.CreditosMax.HasValue)
            {
                cursos = cursos.Where(c => c.Creditos <= model.Filtros.CreditosMax.Value).ToList();
            }

            // Filtros de horario
            if (!string.IsNullOrEmpty(model.Filtros.HorarioDesde))
            {
                if (TimeSpan.TryParse(model.Filtros.HorarioDesde, out TimeSpan horarioDesde))
                {
                    cursos = cursos.Where(c => c.HorarioInicio >= horarioDesde).ToList();
                }
            }

            if (!string.IsNullOrEmpty(model.Filtros.HorarioHasta))
            {
                if (TimeSpan.TryParse(model.Filtros.HorarioHasta, out TimeSpan horarioHasta))
                {
                    cursos = cursos.Where(c => c.HorarioFin <= horarioHasta).ToList();
                }
            }

            var viewModel = new CatalogoViewModel
            {
                Cursos = cursos,
                Filtros = model.Filtros
            };

            return View(viewModel);
        }

        // GET: Cursos/Details/5 - Guardar en Session el último curso visitado
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cursos = await _cursosService.GetCursosActivosAsync();
            var curso = cursos.FirstOrDefault(c => c.Id == id);

            if (curso == null)
            {
                return NotFound();
            }

            // GUARDAR EN SESSION el último curso visitado (nombre e ID)
            HttpContext.Session.SetString("LastVisitedCourse", curso.Nombre);
            HttpContext.Session.SetInt32("LastVisitedCourseId", curso.Id);
            _logger.LogInformation($"💾 Session guardada: LastVisitedCourse = {curso.Nombre} (ID: {curso.Id})");

            return View(curso);
        }
    }
}
