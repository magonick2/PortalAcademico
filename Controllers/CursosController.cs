using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
// Librerías necesarias para que funcione la Pregunta 4
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache; // Inyectado para Redis

    public CursosController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: Cursos (Con Filtros y Caché Redis 60s)
    public async Task<IActionResult> Index(string buscarNombre, int? buscarCreditos)
    {
        string cacheKey = "CursosActivosList";
        List<Curso> cursos = null;

        // Intentar obtener de la caché
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(cachedData))
        {
            // Si no hay caché, buscamos en DB (manteniendo tu lógica de activos)
            cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

            // Serializamos a JSON para guardar en Redis
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cursos), options);
        }
        else
        {
            // Deserializamos el JSON de Redis
            cursos = JsonSerializer.Deserialize<List<Curso>>(cachedData);
        }

        // Aplicamos tus filtros sobre la lista resultante
        var query = cursos.AsQueryable();

        if (!string.IsNullOrEmpty(buscarNombre))
        {
            query = query.Where(c => c.Nombre.Contains(buscarNombre, StringComparison.OrdinalIgnoreCase));
        }

        if (buscarCreditos.HasValue)
        {
            query = query.Where(c => c.Creditos == buscarCreditos);
        }

        return View(query.ToList());
    }

    // Nuevo método Details con lógica de SESIÓN
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id);
        
        if (curso == null) return NotFound();

        // GUARDAR EN SESIÓN: Requerimiento de la Pregunta 4
        HttpContext.Session.SetString("LastCourseId", curso.Id.ToString());
        HttpContext.Session.SetString("LastCourseName", curso.Nombre);

        return View(curso);
    }

    // --- TUS MÉTODOS EXISTENTES DEL PASO 3 ---

    [Authorize]
    public async Task<IActionResult> Inscribirse(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var yaInscrito = await _context.Matriculas
            .AnyAsync(m => m.CursoId == id && m.UsuarioId == userId);

        if (yaInscrito)
        {
            TempData["Error"] = "Ya te encuentras inscrito en este curso.";
            return RedirectToAction(nameof(Index));
        }

        var matricula = new Matricula
        {
            CursoId = id,
            UsuarioId = userId!,
            FechaRegistro = DateTime.Now,
            Estado = EstadoMatricula.Confirmada
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        // RECOMENDACIÓN: Invalidar caché al inscribirse para actualizar datos
        await _cache.RemoveAsync("CursosActivosList");

        TempData["Mensaje"] = "¡Inscripción realizada con éxito!";
        return RedirectToAction(nameof(MisCursos));
    }

    [Authorize]
    public async Task<IActionResult> MisCursos()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var misMatriculas = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == userId)
            .ToListAsync();

        return View(misMatriculas);
    }
}