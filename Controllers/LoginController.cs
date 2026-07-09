using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TesisPractica.Models;
using TesisPractica.Services;
using Microsoft.EntityFrameworkCore;

namespace TesisPractica.Controllers
{
    public class LoginController : Controller
    {
        private readonly TesisDbContext _context;
        private readonly EmailService _emailService;

        public LoginController(TesisDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string username, string password)
        {
            // Hashear la contraseña ingresada antes de compararla
            string hashedPassword = HashPassword(password);

            // Buscar el usuario en la base de datos
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);

            // Verificar usuario y contraseña
            if (usuario != null && usuario.Contrasena == hashedPassword)
            {
                // Crear lista de claims para autenticación
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.Rol ?? "Usuario") // Rol por defecto
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Guardar autenticación en cookies
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvideContrasena()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> EnviarRecuperacion(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                ViewBag.Error = "Usuario no encontrado. Intente nuevamente o contacte con un administrador.";
                return View("OlvideContrasena");
            }

            // Eliminar tokens previos del usuario
            var existingTokens = _context.PasswordResets.Where(r => r.UserId == usuario.Id).ToList();
            _context.PasswordResets.RemoveRange(existingTokens);
            await _context.SaveChangesAsync();

            // Generar nuevo token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var resetEntry = new PasswordReset
            {
                UserId = usuario.Id,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1)
            };

            _context.PasswordResets.Add(resetEntry);
            await _context.SaveChangesAsync();

            // Enviar email con enlace de recuperación
            var resetLink = Url.Action("RestablecerContrasena", "Login", new { token = token }, Request.Scheme);
            string asunto = "Recuperación de contraseña";
            string mensaje = $"<p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p> <a href='{resetLink}'>Restablecer Contraseña</a>";

            await _emailService.EnviarCorreo(email, asunto, mensaje);

            ViewBag.Mensaje = "Se ha enviado un enlace de recuperación a tu correo.";
            return View("OlvideContrasena");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RestablecerContrasena(string token)
        {
            var resetEntry = _context.PasswordResets.FirstOrDefault(r => r.Token == token && r.Expiration > DateTime.UtcNow);
            if (resetEntry == null)
            {
                TempData["Error"] = "El enlace de recuperación es inválido o ha expirado.";
                return RedirectToAction("OlvideContrasena");
            }

            ViewBag.Token = token;
            return View("RestablecerContrasena");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RestablecerContrasenaConfirmado(string token, string nuevaContrasena)
        {
            var resetEntry = _context.PasswordResets.FirstOrDefault(r => r.Token == token && r.Expiration > DateTime.UtcNow);
            if (resetEntry == null)
            {
                TempData["Error"] = "El enlace de recuperación es inválido o ha expirado.";
                return RedirectToAction("OlvideContrasena");
            }

            var usuario = await _context.Usuarios.FindAsync(resetEntry.UserId);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("OlvideContrasena");
            }

            // Hashear la nueva contraseña antes de guardarla
            usuario.Contrasena = HashPassword(nuevaContrasena);
            _context.PasswordResets.Remove(resetEntry);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tu contraseña ha sido restablecida correctamente. Puedes iniciar sesión con la nueva contraseña.";
            return RedirectToAction("Index");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
