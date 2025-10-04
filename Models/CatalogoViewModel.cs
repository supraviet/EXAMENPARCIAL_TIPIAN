namespace EXAPARCIALALVARO.Models
{
    public class CatalogoViewModel
    {
        public List<Curso> Cursos { get; set; } = new List<Curso>();
        public FiltrosCursos Filtros { get; set; } = new FiltrosCursos();
    }

    public class FiltrosCursos
    {
        public string? Nombre { get; set; }
        public int? CreditosMin { get; set; }
        public int? CreditosMax { get; set; }
        
        // Usar string para los filtros de horario (desde el formulario)
        public string? HorarioDesde { get; set; }
        public string? HorarioHasta { get; set; }
    }
}