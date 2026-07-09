using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Authorize] // 🔒 Requiere autenticación para todo el controlador
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TesisDbContext _context; // 🔹 1) Inyectamos el DbContext

        public HomeController(ILogger<HomeController> logger, TesisDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 2) Traemos el usuario logueado
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);

            // 2.a) Ponemos el rol del usuario en el ViewBag
            ViewBag.RolUsuario = usuario.Rol;   // o .RolDescripcion, según como se llame tu propiedad

            // 3) Traemos las notificaciones del usuario
            var notificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuario.Id)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            // 4) Enviamos las notificaciones a la vista
            ViewBag.Notificaciones = notificaciones;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
