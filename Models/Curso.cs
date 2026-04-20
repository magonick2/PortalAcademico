using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models;

public class Curso
{
    public int Id { get; set; }
    
    [Required]
    public string Codigo { get; set; } = string.Empty;
    
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
    public int Creditos { get; set; }

    public int CupoMaximo { get; set; }
    public TimeSpan HorarioInicio { get; set; }
    public TimeSpan HorarioFin { get; set; }
    public bool Activo { get; set; } = true;
}