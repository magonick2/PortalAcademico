using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

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
}