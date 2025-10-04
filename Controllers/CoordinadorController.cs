using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EXAPARCIALALVARO.Models;
using EXAPARCIALALVARO.Data;
using EXAPARCIALALVARO.Services;

namespace EXAPARCIALALVARO.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursosService _cursosService;
        private readonly ILogger<CoordinadorController> _logger;

        public CoordinadorController(
            ApplicationDbContext context, 
            ICursosService cursosService,
            ILogger<CoordinadorController> logger)
        {
            _context = context;
            _cursosService = cursosService;
            _logger = logger;
        }

        // GET: Coordinador - Dashboard principal
        public async Task<IActionResult> Index()
        {
            var estadisticas = new
            {
                TotalCursos = await _context.Cursos.CountAsync(),
                CursosActivos = await _context.Cursos.CountAsync(c => c.Activo),
                CursosInactivos = await _context.Cursos.CountAsync(c => !c.Activo), // AGREGAR ESTO
                TotalMatriculas = await _context.Matriculas.CountAsync(),
                MatriculasPendientes = await _context.Matriculas.CountAsync(m => m.Estado == EstadoMatricula.Pendiente),
                MatriculasConfirmadas = await _context.Matriculas.CountAsync(m => m.Estado == EstadoMatricula.Confirmada),
                MatriculasCanceladas = await _context.Matriculas.CountAsync(m => m.Estado == EstadoMatricula.Cancelada) // AGREGAR ESTO
            };

            ViewBag.Estadisticas = estadisticas;
            return View();
        }
        // POST: Coordinador/ActivarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = true;
            await _context.SaveChangesAsync();

            // Invalidar cache de cursos
            await _cursosService.InvalidarCacheCursosAsync();

            TempData["Success"] = "Curso activado exitosamente.";
            return RedirectToAction(nameof(ListaCursos));
        }


        // GET: Coordinador/CrearCurso
        public IActionResult CrearCurso()
        {
            return View();
        }

        // POST: Coordinador/CrearCurso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el código ya existe
                    var codigoExistente = await _context.Cursos
                        .AnyAsync(c => c.Codigo == curso.Codigo);

                    if (codigoExistente)
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                        return View(curso);
                    }

                    // Validar horario
                    if (curso.HorarioInicio >= curso.HorarioFin)
                    {
                        ModelState.AddModelError("HorarioInicio", "El horario de inicio debe ser anterior al horario de fin.");
                        return View(curso);
                    }

                    curso.Activo = true;
                    _context.Add(curso);
                    await _context.SaveChangesAsync();

                    // Invalidar cache de cursos
                    await _cursosService.InvalidarCacheCursosAsync();

                    TempData["Success"] = "Curso creado exitosamente.";
                    return RedirectToAction(nameof(ListaCursos));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear curso");
                    ModelState.AddModelError("", "Error al crear el curso. Por favor, intente nuevamente.");
                }
            }
            return View(curso);
        }

        // GET: Coordinador/ListaCursos
        public async Task<IActionResult> ListaCursos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Matriculas)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(cursos);
        }

        // GET: Coordinador/EditarCurso/5
        public async Task<IActionResult> EditarCurso(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        // POST: Coordinador/EditarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id, Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el código ya existe (excluyendo el curso actual)
                    var codigoExistente = await _context.Cursos
                        .AnyAsync(c => c.Codigo == curso.Codigo && c.Id != id);

                    if (codigoExistente)
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                        return View(curso);
                    }

                    // Validar horario
                    if (curso.HorarioInicio >= curso.HorarioFin)
                    {
                        ModelState.AddModelError("HorarioInicio", "El horario de inicio debe ser anterior al horario de fin.");
                        return View(curso);
                    }

                    _context.Update(curso);
                    await _context.SaveChangesAsync();

                    // Invalidar cache de cursos
                    await _cursosService.InvalidarCacheCursosAsync();

                    TempData["Success"] = "Curso actualizado exitosamente.";
                    return RedirectToAction(nameof(ListaCursos));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(curso);
        }

        // POST: Coordinador/DesactivarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = false;
            await _context.SaveChangesAsync();

            // Invalidar cache de cursos
            await _cursosService.InvalidarCacheCursosAsync();

            TempData["Success"] = "Curso desactivado exitosamente.";
            return RedirectToAction(nameof(ListaCursos));
        }

        public async Task<IActionResult> Matriculas(int? cursoId)
{
    var query = _context.Matriculas
        .Include(m => m.Curso)
        .Include(m => m.Usuario) // ← ESTA LÍNEA ES CRUCIAL
        .AsQueryable();

    if (cursoId.HasValue)
    {
        query = query.Where(m => m.CursoId == cursoId.Value);
        ViewBag.CursoSeleccionado = await _context.Cursos.FindAsync(cursoId.Value);
    }

    var matriculas = await query
        .OrderByDescending(m => m.FechaRegistro)
        .ToListAsync();

    ViewBag.Cursos = await _context.Cursos
        .Where(c => c.Activo)
        .OrderBy(c => c.Nombre)
        .ToListAsync();

    return View(matriculas);
}

        // POST: Coordinador/ConfirmarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null)
            {
                return NotFound();
            }

            // Verificar que no exceda el cupo máximo
            var cupoOcupado = await _context.Matriculas
                .CountAsync(m => m.CursoId == matricula.CursoId && m.Estado == EstadoMatricula.Confirmada);

            if (cupoOcupado >= matricula.Curso.CupoMaximo)
            {
                TempData["Error"] = "No se puede confirmar la matrícula: el curso ha alcanzado su cupo máximo.";
                return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
            }

            matricula.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula confirmada exitosamente.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
        }

        // POST: Coordinador/CancelarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula cancelada exitosamente.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }
    }
}