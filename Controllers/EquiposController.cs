using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    // DTO para actualizar un equipo
    public class EquipoEditDto
    {
        public int IdEquipo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<UsuarioDto> Usuarios { get; set; }
    }

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Rol { get; set; }
    }

    public class EquiposController : Controller
    {
        private readonly TesisDbContext _context;
        public EquiposController(TesisDbContext context)
        {
            _context = context;
        }

        // GET: /Equipos?persona=123&fecha=2026-04-16
        [HttpGet]
        public async Task<IActionResult> Index(int? persona, DateTime? fecha)
        {
            // 1. Obtengo el usuario actual y su rol
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null)
                return RedirectToAction("Index", "Login");

            ViewBag.RolUsuario = usuario.Rol;
            bool esSupervisor = usuario.Rol == "supervisor";

            List<int> equipoIds;

            // 2. Calculo los IDs de equipo que este usuario puede ver
            if (esSupervisor)
            {
                // Supervisor ve los equipos que él creó
                var query = _context.Equipos
                    .Where(e => e.IdCreador == usuario.Id)
                    .Select(e => e.IdEquipo);

                // Si filtró por empleado, reducir a aquellos donde participa ese empleado
                if (persona.HasValue)
                {
                    int empId = persona.Value;
                    query = query
                        .Where(id => _context.EquiposUsuarios
                            .Any(eu => eu.IdEquipo == id && eu.IdUsuario == empId));
                }

                equipoIds = await query.Distinct().ToListAsync();
            }
            else
            {
                // Empleado ve los equipos donde participa
                var propiosIds = await _context.EquiposUsuarios
                    .Where(eu => eu.IdUsuario == usuario.Id)
                    .Select(eu => eu.IdEquipo)
                    .ToListAsync();

                if (persona.HasValue)
                {
                    int supId = persona.Value;
                    var creadosPorSup = await _context.Equipos
                        .Where(e => e.IdCreador == supId)
                        .Select(e => e.IdEquipo)
                        .ToListAsync();

                    // Solo los que él participa y fueron creados por ese supervisor
                    propiosIds = propiosIds.Intersect(creadosPorSup).ToList();
                }

                equipoIds = propiosIds.Distinct().ToList();
            }

            // 3. Recupero los equipos (solo un Include)
            var equiposQuery = _context.Equipos
                .Where(e => equipoIds.Contains(e.IdEquipo));

            // 4. Aplico filtro de fecha si vino
            if (fecha.HasValue)
            {
                var d = fecha.Value.Date;
                equiposQuery = equiposQuery
                    .Where(e => e.FechaCreacion.Date == d);
            }

            var equipos = await equiposQuery
                .Include(e => e.EquiposUsuarios)
                    .ThenInclude(eu => eu.Usuario)
                .OrderBy(e => e.IdEquipo)
                .ToListAsync();

            // 5. Preparo los dropdowns para la vista
            ViewBag.Empleados = esSupervisor
                ? await _context.Usuarios
                    .Where(u => u.Rol == "empleado")
                    .OrderBy(u => u.NombreUsuario)
                    .ToListAsync()
                : new List<Usuario>();

            ViewBag.Supervisores = !esSupervisor
                ? await _context.Usuarios
                    .Where(u => u.Rol == "supervisor")
                    .OrderBy(u => u.NombreUsuario)
                    .ToListAsync()
                : new List<Usuario>();
            ViewBag.UsuarioId = usuario.Id;

            return View(equipos);
        }


        // POST: /Equipos/Crear
        [HttpPost]
        public async Task<IActionResult> Crear(
            string nombre,
            string descripcion,
            int? piloto,
            int? fichaTecnica,
            List<int> auditores,
            List<int> empleados)
        {
            if (string.IsNullOrWhiteSpace(nombre)
                || string.IsNullOrWhiteSpace(descripcion)
                || !piloto.HasValue)
                return RedirectToAction("Index");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null)
                return RedirectToAction("Index", "Login");

            var nuevoEquipo = new Equipo
            {
                Nombre = nombre,
                Descripcion = descripcion,
                IdCreador = usuario.Id,
                FechaCreacion = DateTime.Now
            };
            _context.Equipos.Add(nuevoEquipo);
            await _context.SaveChangesAsync();

            var relaciones = new List<Equipos_Usuarios>();
            var usados = new HashSet<int>();

            // Piloto
            if (usados.Add(piloto.Value))
                relaciones.Add(new Equipos_Usuarios
                {
                    IdEquipo = nuevoEquipo.IdEquipo,
                    IdUsuario = piloto.Value,
                    RolEnEquipo = "Piloto"
                });

            // Auditores
            foreach (var idA in auditores ?? new List<int>())
                if (usados.Add(idA))
                    relaciones.Add(new Equipos_Usuarios
                    {
                        IdEquipo = nuevoEquipo.IdEquipo,
                        IdUsuario = idA,
                        RolEnEquipo = "Auditor"
                    });

            // Empleados
            foreach (var idE in empleados ?? new List<int>())
                if (usados.Add(idE))
                    relaciones.Add(new Equipos_Usuarios
                    {
                        IdEquipo = nuevoEquipo.IdEquipo,
                        IdUsuario = idE,
                        RolEnEquipo = "Empleado"
                    });

            _context.EquiposUsuarios.AddRange(relaciones);
            await _context.SaveChangesAsync();

            TempData["EquipoCreado"] = "¡Equipo creado correctamente!";
            return RedirectToAction("Index");
        }

        // POST: /Equipos/ActualizarEquipo
        [HttpPost]
        public async Task<IActionResult> ActualizarEquipo([FromBody] EquipoEditDto dto)
        {
            if (dto == null) return BadRequest("Datos incorrectos.");

            var equipo = await _context.Equipos
                .Include(e => e.EquiposUsuarios)
                .FirstOrDefaultAsync(e => e.IdEquipo == dto.IdEquipo);
            if (equipo == null) return NotFound();

            equipo.Nombre = dto.Nombre;
            equipo.Descripcion = dto.Descripcion;

            _context.EquiposUsuarios.RemoveRange(equipo.EquiposUsuarios);
            var nuevas = dto.Usuarios
                .Select(u => new Equipos_Usuarios
                {
                    IdEquipo = equipo.IdEquipo,
                    IdUsuario = u.Id,
                    RolEnEquipo = u.Rol
                })
                .ToList();
            _context.EquiposUsuarios.AddRange(nuevas);

            await _context.SaveChangesAsync();
            return Json(new { message = "Equipo actualizado correctamente" });
        }

        // GET: /Equipos/ValidarNombre?nombre=XXX
        [HttpGet]
        public async Task<IActionResult> ValidarNombre(string nombre, int? idCreador)
        {
            if (string.IsNullOrWhiteSpace(nombre) || !idCreador.HasValue)
                return Json(new { disponible = false });

            bool existe = await _context.Equipos
                .AnyAsync(e => e.Nombre == nombre && e.IdCreador == idCreador.Value);

            return Json(new { disponible = !existe });
        }


        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null) return NotFound();
            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Devuelve la lista de equipos creados por el usuario en sesión.
        /// </summary>
        [HttpGet("Equipos/GetEquiposByCreador")]
        public async Task<IActionResult> GetEquiposByCreador()
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Json(new object[0]);

            var equipos = await _context.Equipos
                .Where(e => e.IdCreador == usuario.Id)
                .Select(e => new { e.IdEquipo, e.Nombre })
                .ToListAsync();

            return Json(equipos);
        }

        /// <summary>
        /// Dada la clave de un equipo, devuelve los usuarios que forman parte de él.
        /// </summary>
        [HttpGet("Equipos/GetMiembrosDeEquipo")]
        public async Task<IActionResult> GetMiembrosDeEquipo(int idEquipo)
        {
            var miembros = await _context.EquiposUsuarios
                .Where(eu => eu.IdEquipo == idEquipo)
                .Include(eu => eu.Usuario)
                .Select(eu => new { eu.Usuario.Id, eu.Usuario.NombreUsuario })
                .ToListAsync();

            return Json(miembros);
        }

        // GET: /Equipos/GetFichasDeEquipo?idEquipo=123
        [HttpGet]
        public async Task<IActionResult> GetFichasDeEquipo(int idEquipo)
        {
            // 1) Usuario actual y rol
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Unauthorized();

            bool esSupervisor = usuario.Rol.Equals("supervisor", StringComparison.OrdinalIgnoreCase);

            // 2) Base de la consulta: FT que pertenecen a este equipo
            var query = _context.FichasTecnicas
                .Include(ft => ft.NoConformidad)
                .Where(ft => ft.IdEquipo == idEquipo);

            // 3) Si es supervisor, filtro para que solo vea las FT de NCs que él creó
            if (esSupervisor)
                query = query.Where(ft => ft.NoConformidad.IdUsuario == usuario.Id);

            // 4) Proyección
            var lista = await query
                .Select(ft => new {
                    ft.IdFichaTecnica,
                    ft.IdNoConformidad,
                    Fecha = ft.FechaCreacion.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(lista);
        }

        // Devuelve las FT sin equipo, sólo de NC creadas por este supervisor
        [HttpGet]
        public async Task<IActionResult> GetFTSinAsignar(int equipoId)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            bool esSupervisor = usuario.Rol.Equals("supervisor", StringComparison.OrdinalIgnoreCase);

            var q = _context.FichasTecnicas
                    .Include(ft => ft.NoConformidad)
                    .Where(ft => ft.IdEquipo == null);

            if (esSupervisor)
                q = q.Where(ft => ft.NoConformidad.IdUsuario == usuario.Id);

            var lista = await q
                .Select(ft => new {
                    ft.IdFichaTecnica
                })
                .ToListAsync();

            return Json(lista);
        }

        // Asigna la FT al equipo
        [HttpPost]
        public async Task<IActionResult> AsignarFT([FromBody] AssignFTDto dto)
        {
            var ft = await _context.FichasTecnicas.FindAsync(dto.IdFichaTecnica);
            if (ft == null) return NotFound();
            ft.IdEquipo = dto.IdEquipo;
            await _context.SaveChangesAsync();
            return Json(new { message = "Ficha técnica asignada correctamente" });
        }

        public class AssignFTDto
        {
            public int IdEquipo { get; set; }
            public int IdFichaTecnica { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> DesasignarFT([FromBody] AssignFTDto dto)
        {
            var ft = await _context.FichasTecnicas.FindAsync(dto.IdFichaTecnica);
            if (ft == null) return NotFound();
            ft.IdEquipo = null;
            await _context.SaveChangesAsync();
            return Json(new { message = "Ficha técnica desasignada correctamente" });
        }



    }
}
