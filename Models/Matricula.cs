using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // Agregar este using

namespace EXAPARCIALALVARO.Models
{
    public class Matricula
    {
        public int Id { get; set; }
        
        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;
        
        public string UsuarioId { get; set; } = string.Empty;
        
        // AGREGAR esta propiedad para la relación con IdentityUser
        [ForeignKey("UsuarioId")]
        public virtual IdentityUser? Usuario { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }

    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }
}