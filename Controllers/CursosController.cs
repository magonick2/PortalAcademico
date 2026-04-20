using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using Microsoft.AspNetCore.Authorization; // Agregado para seguridad
using System.Security.Claims;             // Agregado para obtener ID del usuario

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Cursos (Con Filtros)
    public async Task<IActionResult> Index(string buscarNombre, int? buscarCreditos)
    {
        var cursosQuery = _context.Cursos.Where(c => c.Activo);

        if (!string.IsNullOrEmpty(buscarNombre))
        {
            cursosQuery = cursosQuery.Where(c => c.Nombre.Contains(buscarNombre));
        }

        if (buscarCreditos.HasValue)
        {
            cursosQuery = cursosQuery.Where(c => c.Creditos == buscarCreditos);
        }

        return View(await cursosQuery.ToListAsync());
    }

    // --- MÉTODOS AGREGADOS PARA EL PASO 3 ---

    // Acción para procesar la inscripción
    [Authorize] // Solo usuarios logueados
    public async Task<IActionResult> Inscribirse(int id)
    {
        // 1. Obtener ID del usuario logueado
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. VALIDACIÓN: Verificar si ya existe la matrícula para este curso y usuario
        var yaInscrito = await _context.Matriculas
            .AnyAsync(m => m.CursoId == id && m.UsuarioId == userId);

        if (yaInscrito)
        {
            // Enviamos alerta al Index si ya está inscrito
            TempData["Error"] = "Ya te encuentras inscrito en este curso.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Crear el registro de Matrícula
        var matricula = new Matricula
        {
            CursoId = id,
            UsuarioId = userId!,
            FechaRegistro = DateTime.Now,
            Estado = EstadoMatricula.Confirmada
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        // 4. Mensaje de éxito
        TempData["Mensaje"] = "¡Inscripción realizada con éxito!";
        return RedirectToAction(nameof(MisCursos));
    }

    // Acción para ver la lista de cursos del alumno
    [Authorize]
    public async Task<IActionResult> MisCursos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Traemos las matrículas del usuario actual incluyendo los datos del objeto Curso
        var misMatriculas = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == userId)
            .ToListAsync();

        return View(misMatriculas);
    }
}