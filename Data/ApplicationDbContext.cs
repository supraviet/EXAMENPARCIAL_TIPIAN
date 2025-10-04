using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EXAPARCIALALVARO.Models;

namespace EXAPARCIALALVARO.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración para Curso
            builder.Entity<Curso>(entity =>
            {
                entity.HasIndex(c => c.Codigo).IsUnique();
                entity.HasCheckConstraint("CK_Curso_HorarioValido", "HorarioInicio < HorarioFin");
                entity.HasCheckConstraint("CK_Curso_CreditosPositivos", "Creditos > 0");
            });

            // Configuración para Matricula
            builder.Entity<Matricula>(entity =>
            {
                // Un usuario no puede estar matriculado más de una vez en el mismo curso
                entity.HasIndex(m => new { m.CursoId, m.UsuarioId }).IsUnique();

                // Relación con Curso
                entity.HasOne(m => m.Curso)
                      .WithMany(c => c.Matriculas)
                      .HasForeignKey(m => m.CursoId)
                      .OnDelete(DeleteBehavior.Cascade);

                // RELACIÓN CON IDENTITYUSER - AGREGAR ESTO
                entity.HasOne(m => m.Usuario)
                      .WithMany() // IdentityUser no tiene colección de Matriculas
                      .HasForeignKey(m => m.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data - Cursos iniciales
            builder.Entity<Curso>().HasData(
                new Curso
                {
                    Id = 1,
                    Codigo = "MAT101",
                    Nombre = "Matemáticas Básicas",
                    Creditos = 4,
                    CupoMaximo = 30,
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(10, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Id = 2,
                    Codigo = "PROG101",
                    Nombre = "Programación I",
                    Creditos = 5,
                    CupoMaximo = 25,
                    HorarioInicio = new TimeSpan(10, 0, 0),
                    HorarioFin = new TimeSpan(12, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Id = 3,
                    Codigo = "BD101",
                    Nombre = "Bases de Datos",
                    Creditos = 4,
                    CupoMaximo = 20,
                    HorarioInicio = new TimeSpan(14, 0, 0),
                    HorarioFin = new TimeSpan(16, 0, 0),
                    Activo = true
                }
            );
        }
    }
}