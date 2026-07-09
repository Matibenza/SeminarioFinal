using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    public class ComentarioDto
    {
        public int IdTarea { get; set; }
        public string Texto { get; set; }
    }

    public class TareaDto
    {
        public int IdTarea { get; set; }
        public int IdFichaTecnica { get; set; }
        public int IdCausaPasos { get; set; }
        public string AccionDeMejora { get; set; }
        public string DescripcionDeMejora { get; set; }
        public string CanalImplementacion { get; set; }
        public string LugarAplicacion { get; set; }
        public DateTime? FechaObjetivo { get; set; }
        public DateTime? FechaFinal { get; set; }
        public int IdResponsable { get; set; }
        public int IdEstado { get; set; }
        public int IdPrioridad { get; set; }
        public int IdTipoAccion { get; set; }
    }
    [Route("FichasTecnicas/{idFicha}/Paso4")]
    public class Paso4Controller : Controller
    {
        private readonly TesisDbContext _context;
        public Paso4Controller(TesisDbContext context) => _context = context;

        // GET: carga o inicializa Paso4, prepara permisos y dropdowns
        [HttpGet("")]
        public async Task<IActionResult> Index(int idFicha)
        {
            // 1) Cargo ficha con Paso4, Paso3 y Equipo→Usuarios
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso4)
                .Include(f => f.Paso3)
                .Include(f => f.Equipo)
                   .ThenInclude(e => e.EquiposUsuarios)
                      .ThenInclude(eu => eu.Usuario)
                .Include(f => f.NoConformidad)
                   .ThenInclude(nc => nc.Pieza)
                   .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);

            if (ficha == null)
                return NotFound();

            // 2) Inicializo Paso4 en memoria si no existe
            if (ficha.Paso4 == null)
            {
                var fechaBase = ficha.Paso3?.FechaFinEstimada ?? DateTime.Now;
                ficha.Paso4 = new Paso4
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = DateTime.Now,
                    FechaFinEstimada = fechaBase.AddDays(7)
                };
            }

            // 3) Flags de permisos y estado de Paso3
            // 3.a) Determinar si el Paso 3 está COMPLETADO
            var estadoTerminado = await _context.Estados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Descripcion == "Terminado");

            ViewBag.Paso3Terminado = ficha.Paso3 != null
                && estadoTerminado != null
                && ficha.Paso3.IdEstado == estadoTerminado.IdEstado;

            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null) return Unauthorized();

            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?
                .RolEnEquipo.ToLower() ?? "";

            ViewBag.EsPiloto = rol == "piloto";
            ViewBag.EsAuditor = rol == "auditor";
            ViewBag.EsResponsable = ficha.Paso4.Responsable == usuarioActual.Id;
            ViewBag.EsSupervisor = usuarioActual.Rol
                                     .Equals("supervisor", StringComparison.OrdinalIgnoreCase);

            // 4) Estados para la vista principal y para el modal (sin "Terminado")
            var estadosAll = await _context.Estados.AsNoTracking().ToListAsync();
            ViewBag.Estados = new SelectList(
                estadosAll,
                "IdEstado", "Descripcion",
                ficha.Paso4.IdEstado
            );
            var estadosModal = estadosAll
                .Where(e => !e.Descripcion.Equals("Terminado",
                           StringComparison.OrdinalIgnoreCase))
                .ToList();
            ViewBag.EstadosModal = new SelectList(
                estadosModal,
                "IdEstado", "Descripcion",
                ficha.Paso4.IdEstado
            );

            // 5) Prioridades y tipos de acción para el modal de tarea
            var prioridadesTarea = await _context.Prioridades
                .AsNoTracking()
                .ToListAsync();
            ViewBag.PrioridadesTarea = prioridadesTarea
                .Select(p => new SelectListItem
                {
                    Value = p.IdPrioridad.ToString(),
                    Text = p.Descripcion
                })
                .ToList();

            var tiposAccion = await _context.TiposAcciones
                .AsNoTracking()
                .ToListAsync();
            ViewBag.TiposAccionTarea = tiposAccion
                .Select(t => new SelectListItem
                {
                    Value = t.IdTipoAccion.ToString(),
                    Text = t.Descripcion
                })
                .ToList();

            // 6) Dropdown principal de Responsable (sólo el Piloto)
            var piloto = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo
                    .Equals("Piloto", StringComparison.OrdinalIgnoreCase))
                ?.Usuario;
            if (piloto != null)
            {
                ViewBag.Responsables4 = new SelectList(
                    new[] { piloto },
                    "Id", "NombreUsuario",
                    piloto.Id
                );
                ficha.Paso4.Responsable = piloto.Id;
            }
            else
            {
                ViewBag.Responsables4 = new SelectList(
                    Enumerable.Empty<Usuario>(),
                    "Id", "NombreUsuario"
                );
            }

            // 7) Empleados del equipo para el modal de tarea
            ViewBag.EmpleadosTarea = ficha.Equipo.EquiposUsuarios
                .Select(eu => new SelectListItem
                {
                    Value = eu.IdUsuario.ToString(),
                    Text = eu.Usuario.NombreUsuario
                })
                .ToList();

            // 8) Causas/fenómenos (5º Porqué) para el modal de tarea
            ViewBag.CausasFenomeno = await _context.Analisis5Porques
                .Where(a => a.IdFichaTecnica == idFicha)
                .Select(a => new SelectListItem
                {
                    Value = a.IdCausa.ToString(),  // o el PK que prefieras
                    Text = a.QuintoPorque                   // el campo que quieres mostrar
                })
                .ToListAsync();

            // 9) Tareas de la ficha para el tablero
            var tareas = await _context.Tareas
                .Where(t => t.IdFichaTecnica == idFicha)
                .Include(t => t.Estado)
                .Include(t => t.Prioridad)
                .OrderBy(t => t.FechaObjetivo)
                .ToListAsync();
            ViewBag.TareasPaso4 = tareas;

            // 10) Filtros: prioridades (todas) y personas (sólo del equipo)
            ViewBag.PrioridadesFiltro = new SelectList(
                prioridadesTarea.OrderBy(p => p.Descripcion),
                "IdPrioridad", "Descripcion"
            );
            ViewBag.PersonasFiltro = new SelectList(
                ficha.Equipo.EquiposUsuarios
                    .Select(eu => new {
                        Id = eu.Usuario.Id,
                        Name = eu.Usuario.NombreUsuario
                    })
                    .OrderBy(x => x.Name)
                    .ToList(),
                "Id", "Name"
            );

            // 11) Devuelvo la vista
            return View("~/Views/FichasTecnicas/Paso4.cshtml", ficha);
        }


        // POST: guarda Paso4 y vuelve a Detalle
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            int idFicha,
            [Bind("FechaInicio,FechaFinEstimada,FechaFin,Responsable,IdEstado", Prefix = "Paso4")]
    Paso4 paso4Model)
        {
            // 1) Cargo ficha con todo lo necesario
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso4)
                .Include(f => f.Paso3)
                .Include(f => f.Equipo)
                   .ThenInclude(e => e.EquiposUsuarios)
                      .ThenInclude(eu => eu.Usuario)
                .Include(f => f.NoConformidad)
                   .ThenInclude(nc => nc.Pieza)
                      .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            // 2) Usuario actual y rol en el equipo
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null) return Unauthorized();

            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?
                .RolEnEquipo.ToLower() ?? "";

            // 3) Si hay errores de validación, recargo ViewBags y devuelvo la vista
            if (!ModelState.IsValid)
            {
                // — flags de permisos y estado de Paso3
                ViewBag.Paso3Terminado = ficha.Paso3 != null;
                ViewBag.EsPiloto = rol == "piloto";
                ViewBag.EsAuditor = rol == "auditor";
                ViewBag.EsResponsable = ficha.Paso4?.Responsable == usuarioActual.Id;
                ViewBag.EsSupervisor = usuarioActual.Rol.Equals("supervisor", StringComparison.OrdinalIgnoreCase);

                // — todos los estados (para dropdown principal)
                var estadosAll = await _context.Estados.AsNoTracking().ToListAsync();
                ViewBag.Estados = new SelectList(estadosAll, "IdEstado", "Descripcion", ficha.Paso4?.IdEstado);

                // — estados para modal (sin "Terminado")
                var estadosModal = estadosAll
                    .Where(e => !e.Descripcion.Equals("Terminado", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                ViewBag.EstadosModal = new SelectList(estadosModal, "IdEstado", "Descripcion", ficha.Paso4?.IdEstado);

                // — prioridades y tipos de acción para el modal de tarea
                ViewBag.PrioridadesTarea = new SelectList(
                    await _context.Prioridades.AsNoTracking().ToListAsync(),
                    "IdPrioridad", "Descripcion");
                ViewBag.TiposAccionTarea = (await _context.TiposAcciones.AsNoTracking().ToListAsync())
                    .Select(t => new SelectListItem
                    {
                        Value = t.IdTipoAccion.ToString(),
                        Text = t.Descripcion
                    })
                    .ToList();

                // — sólo el piloto en el dropdown de responsables del paso 4
                var pilotoErr = ficha.Equipo.EquiposUsuarios
                    .FirstOrDefault(eu => eu.RolEnEquipo.Equals("Piloto", StringComparison.OrdinalIgnoreCase))
                    ?.Usuario;
                ViewBag.Responsables4 = pilotoErr != null
                    ? new SelectList(new[] { pilotoErr }, "Id", "NombreUsuario", pilotoErr.Id)
                    : new SelectList(Enumerable.Empty<Usuario>(), "Id", "NombreUsuario");

                // — causas disponibles para tareas
                var analizadosErr = await _context.Analisis5Porques
                    .Where(a => a.IdFichaTecnica == idFicha)
                    .Select(a => a.IdCausa)
                    .ToListAsync();
                ViewBag.CausasFenomeno = await _context.CausasPasos
                    .Where(cp => cp.IdFichaTecnica == idFicha && !analizadosErr.Contains(cp.IdCausa))
                    .Select(cp => new SelectListItem
                    {
                        Value = cp.IdCausa.ToString(),
                        Text = cp.DescripcionCausa
                    })
                    .ToListAsync();

                // — empleados para el modal de tarea
                ViewBag.EmpleadosTarea = ficha.Equipo.EquiposUsuarios
                    .Select(eu => new SelectListItem
                    {
                        Value = eu.IdUsuario.ToString(),
                        Text = eu.Usuario.NombreUsuario
                    })
                    .ToList();

                return View("~/Views/FichasTecnicas/Paso4.cshtml", ficha);
            }

            // 4) Capturo el estado anterior para la lógica de notificaciones
            int? estadoAnterior = ficha.Paso4?.IdEstado;

            // 5) Agrego o actualizo el Paso4
            if (ficha.Paso4 == null)
            {
                paso4Model.IdFichaTecnica = idFicha;
                paso4Model.FechaCreacion = DateTime.Now;
                _context.Paso4.Add(paso4Model);
                ficha.Paso4 = paso4Model;
            }
            else
            {
                // si el auditor edita, no le dejo cambiar el responsable
                if (rol == "auditor")
                    paso4Model.Responsable = ficha.Paso4.Responsable;

                ficha.Paso4.FechaInicio = paso4Model.FechaInicio;
                ficha.Paso4.FechaFinEstimada = paso4Model.FechaFinEstimada;
                ficha.Paso4.FechaFin = paso4Model.FechaFin;
                ficha.Paso4.Responsable = paso4Model.Responsable;
                ficha.Paso4.IdEstado = paso4Model.IdEstado;
            }

            await _context.SaveChangesAsync();

            // 6) Preparo los estados “COMPLETADO” y “TERMINADO”
            var completado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var terminado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            // 7) Localizo al piloto
            var pilotoUsuario = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")
                ?.Usuario;

            // 8) Si acabo de pasar a COMPLETADO, notifico al piloto (una sola vez)
            if (completado != null
                && ficha.Paso4.IdEstado == completado.IdEstado
                && estadoAnterior != completado.IdEstado
                && pilotoUsuario != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"El responsable completó el Paso 4 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = pilotoUsuario.Id
                });
            }

            // 9) Si acabo de pasar a TERMINADO, notifico a todos los auditores (menos al piloto)
            if (terminado != null
                && ficha.Paso4.IdEstado == terminado.IdEstado
                && estadoAnterior != terminado.IdEstado)
            {
                var auditores = ficha.Equipo.EquiposUsuarios
                    .Where(eu => eu.RolEnEquipo.ToLower() == "auditor"
                              && eu.Usuario.Id != pilotoUsuario?.Id)
                    .Select(eu => eu.Usuario);

                foreach (var aud in auditores)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        Mensaje = $"Paso 4 de la ficha técnica {idFicha} está terminado. Ya está habilitado el Paso 5.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = aud.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            // 10) Redirijo a la vista de detalle
            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });
        }


        [HttpPost("CrearTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTarea(int idFicha, [Bind] Tarea tarea)
        {
            // Aseguramos que la FK venga correcta
            tarea.IdFichaTecnica = idFicha;

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { idFicha });
        }


        [HttpPost("ActualizarEstadoTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoTarea(int idTarea, string nuevoEstado)
        {
            // 1) Buscamos la tarea
            var tarea = await _context.Tareas
                .FirstOrDefaultAsync(t => t.IdTarea == idTarea);
            if (tarea == null) return NotFound();

            // 2) Comparamos sin StringComparison, usando ToUpper para SQL
            var nuevo = nuevoEstado.ToUpper();
            var estado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == nuevo);
            if (estado == null) return BadRequest("Estado desconocido");

            // 3) Asignamos y guardamos
            tarea.IdEstado = estado.IdEstado;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("GetComentariosTarea")]
        public async Task<IActionResult> GetComentariosTarea(int idTarea)
        {
            var comentarios = await _context.Comentarios
                .Where(c => c.IdTarea == idTarea)
                .Select(c => new {
                    autor = c.Usuario.NombreUsuario,
                    texto = c.Descripcion,
                    fecha = c.FechaCreacion.ToString("o")
                   
                })
                .ToListAsync();

            return Json(comentarios);
        }


        [HttpPost("AgregarComentario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(ComentarioDto dto)
        {
            if (dto == null || dto.IdTarea <= 0 || string.IsNullOrWhiteSpace(dto.Texto))
                return BadRequest("Datos inválidos.");

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Unauthorized();

            var comment = new Comentario
            {
                IdTarea = dto.IdTarea,
                Descripcion = dto.Texto,
                UsuarioId = usuario.Id,
                FechaCreacion = DateTime.Now
            };
            _context.Comentarios.Add(comment);
            await _context.SaveChangesAsync();

            // Devolvemos JSON con el comentario recién creado
            return Json(new
            {
                autor = usuario.NombreUsuario,
                fecha = comment.FechaCreacion.ToString("yyyy-MM-dd HH:mm"),
                texto = comment.Descripcion
            });
        }

        [HttpGet("GetTarea")]
        public async Task<JsonResult> GetTarea(int idTarea)
        {
            var t = await _context.Tareas
                .Where(x => x.IdTarea == idTarea)
                .Select(x => new {
                    // — campos que ya tenías —
                    accionDeMejora = x.AccionDeMejora,
                    descripcionMejora = x.DescripcionDeMejora,
                    canal = x.CanalImplementacion,
                    lugar = x.LugarAplicacion,
                    fechaObjetivo = x.FechaObjetivo,
                    fechaFinal = x.FechaFinal,
                    idResponsable = x.IdResponsable,
                    idEstado = x.IdEstado,
                    idPrioridad = x.IdPrioridad,
                    idTipoAccion = x.IdTipoAccion,
                    idTarea = x.IdTarea,
                    idFichaTecnica = x.IdFichaTecnica,
                    idCausaPasos = x.IdCausaPasos
                })
                .FirstOrDefaultAsync();

            return Json(t);
        }


        [HttpPost("EliminarTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarTarea(int idTarea)
        {
            var tarea = await _context.Tareas.FindAsync(idTarea);
            if (tarea == null) return NotFound();
            // Eliminar comentarios asociados
            _context.Comentarios.RemoveRange(
                _context.Comentarios.Where(c => c.IdTarea == idTarea)
            );
            // Eliminar la tarea
            _context.Tareas.Remove(tarea);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("EditarTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTarea(TareaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tarea = await _context.Tareas.FindAsync(dto.IdTarea);
            if (tarea == null)
                return NotFound();

            // Actualizo solo si vinieron valores
            tarea.IdCausaPasos = dto.IdCausaPasos;
            tarea.AccionDeMejora = dto.AccionDeMejora;
            tarea.DescripcionDeMejora = dto.DescripcionDeMejora;
            tarea.CanalImplementacion = dto.CanalImplementacion;
            tarea.LugarAplicacion = dto.LugarAplicacion;

            if (dto.FechaObjetivo.HasValue)
                tarea.FechaObjetivo = dto.FechaObjetivo.Value;

            if (dto.FechaFinal.HasValue)
                tarea.FechaFinal = dto.FechaFinal.Value;

            tarea.IdResponsable = dto.IdResponsable;
            tarea.IdEstado = dto.IdEstado;
            tarea.IdPrioridad = dto.IdPrioridad;
            tarea.IdTipoAccion = dto.IdTipoAccion;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { idFicha = dto.IdFichaTecnica });
        }
    }
}

