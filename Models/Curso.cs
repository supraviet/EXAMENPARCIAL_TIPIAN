using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EXAPARCIALALVARO.Utilities.Attributes;  // ← AGREGAR este using

namespace EXAPARCIALALVARO.Models
{
    [HorarioValido]  // ← AGREGAR este atributo a la clase
    public class Curso
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Curso")]
        public string Nombre { get; set; } = string.Empty;
        
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        [Display(Name = "Créditos")]
        public int Creditos { get; set; }
        
        [Range(1, 100, ErrorMessage = "El cupo máximo debe estar entre 1 y 100")]
        [Display(Name = "Cupo Máximo")]
        public int CupoMaximo { get; set; }
        
        [DataType(DataType.Time)]
        [Display(Name = "Horario de Inicio")]
        public TimeSpan HorarioInicio { get; set; }
        
        [DataType(DataType.Time)]
        [Display(Name = "Horario de Fin")]
        public TimeSpan HorarioFin { get; set; }
        
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
        
        // Navigation property
        public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
        
        [NotMapped]
        public int CupoDisponible => CupoMaximo - Matriculas.Count(m => m.Estado == EstadoMatricula.Confirmada);
    }
}