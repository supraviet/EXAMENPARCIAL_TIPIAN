using System.ComponentModel.DataAnnotations;
using EXAPARCIALALVARO.Models;

namespace EXAPARCIALALVARO.Utilities.Attributes
{
    public class HorarioValidoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var curso = (Curso)validationContext.ObjectInstance;
            
            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                return new ValidationResult("El horario de inicio debe ser anterior al horario de fin.");
            }
            
            return ValidationResult.Success;
        }
    }
}