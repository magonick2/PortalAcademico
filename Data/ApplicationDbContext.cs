using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models; // Importante añadir esto

namespace PortalAcademico.Data;

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
        
        // Restricción: Código de curso único
        builder.Entity<Curso>()
            .HasIndex(c => c.Codigo)
            .IsUnique();
    }
}