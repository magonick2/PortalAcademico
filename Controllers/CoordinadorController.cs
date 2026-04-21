using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using Microsoft.AspNetCore.Authorization;

namespace PortalAcademico.Controllers
{
    [Authorize] // Solo usuarios registrados pueden entrar
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtenemos todas las matrículas e incluimos los datos del curso
            var lista = await _context.Matriculas
                .Include(m => m.Curso)
                .ToListAsync();
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula != null)
            {
                _context.Matriculas.Remove(matricula);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}