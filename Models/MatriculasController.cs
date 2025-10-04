using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EXAPARCIALALVARO.Models;
using EXAPARCIALALVARO.Data;

namespace EXAPARCIALALVARO.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<MatriculasController> _logger;

        public MatriculasController(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager,
            ILogger<MatriculasController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Matriculas/Inscribir/5
        public async Task<IActionResult> Inscribir(int? cursoId)
        {
            if (cursoId == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);

            if (curso == null)
            {
                return NotFound();
            }

            var viewModel = new InscripcionViewModel
            {
                Curso = curso
            };

            return View(viewModel);
        }

        // POST: Matriculas/Inscribir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(int cursoId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);

            if (curso == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge(); // Redirige al login si no está autenticado
            }

            var viewModel = new InscripcionViewModel
            {
                Curso = curso
            };

            // VALIDACIÓN 1: Usuario debe estar autenticado (ya se asegura con [Authorize])
            
            // VALIDACIÓN 2: No superar el CupoMaximo
            var matriculasConfirmadas = curso.Matriculas.Count(m => m.Estado == EstadoMatricula.Confirmada);
            if (matriculasConfirmadas >= curso.CupoMaximo)
            {
                viewModel.Mensaje = "Error: El curso ha alcanzado su cupo máximo.";
                viewModel.EsExitoso = false;
                return View(viewModel);
            }

            // VALIDACIÓN 3: No estar matriculado más de una vez en el mismo curso
            var matriculaExistente = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.UsuarioId == usuario.Id);

            if (matriculaExistente != null)
            {
                viewModel.Mensaje = "Error: Ya estás matriculado en este curso.";
                viewModel.EsExitoso = false;
                return View(viewModel);
            }

            // VALIDACIÓN 4: No solaparse con otro curso ya matriculado en el mismo horario
            var matriculasUsuario = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuario.Id && 
                           m.Estado != EstadoMatricula.Cancelada &&
                           m.Curso.Activo)
                .ToListAsync();

            var tieneSolapamiento = matriculasUsuario.Any(m => 
                SolapanHorarios(m.Curso.HorarioInicio, m.Curso.HorarioFin, 
                              curso.HorarioInicio, curso.HorarioFin));

            if (tieneSolapamiento)
            {
                viewModel.Mensaje = "Error: El horario de este curso se solapa con otro curso en el que ya estás matriculado.";
                viewModel.EsExitoso = false;
                return View(viewModel);
            }

            // Si pasa todas las validaciones, crear la matrícula
            var nuevaMatricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = usuario.Id,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            try
            {
                _context.Matriculas.Add(nuevaMatricula);
                await _context.SaveChangesAsync();

                viewModel.Mensaje = "¡Inscripción realizada con éxito! Tu matrícula está en estado Pendiente.";
                viewModel.EsExitoso = true;

                _logger.LogInformation($"Usuario {usuario.UserName} se inscribió en el curso {curso.Nombre}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear matrícula");
                viewModel.Mensaje = "Error: Ocurrió un problema al procesar tu inscripción.";
                viewModel.EsExitoso = false;
            }

            return View(viewModel);
        }

        // GET: Matriculas/MisMatriculas
        public async Task<IActionResult> MisMatriculas()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }

        // Método auxiliar para verificar solapamiento de horarios
        private bool SolapanHorarios(TimeSpan inicio1, TimeSpan fin1, TimeSpan inicio2, TimeSpan fin2)
        {
            return inicio1 < fin2 && inicio2 < fin1;
        }
    }
}