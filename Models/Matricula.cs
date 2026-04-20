using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models;

public class Matricula
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public Curso? Curso { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
}

public enum EstadoMatricula { Pendiente, Confirmada, Cancelada }