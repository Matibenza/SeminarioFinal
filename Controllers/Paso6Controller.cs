using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("FichasTecnicas/{idFicha}/Paso6")]
    public class Paso6Controller : Controller
    {
        private readonly TesisDbContext _context;

        public Paso6Controller(TesisDbContext context)
        {
            _context = context;
        }

        // GET: FichasTecnicas/{idFicha}/Paso6
        [HttpGet("")]
        public async Task<IActionResult> Index(int idFicha)
        {
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso6)
                .Include(f => f.Paso5) // por si necesitás calcular fecha estimada desde Paso5
                .Include(f => f.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            // Id del estado TERMINADO (1 sola consulta, async)
            var idEstadoTerminado = await _context.Estados
                .Where(e => e.Descripcion.ToUpper().Trim() == "TERMINADO")
                .Select(e => e.IdEstado)
                .FirstOrDefaultAsync();

            // true si Paso5 existe y su IdEstado es TERMINADO
            bool paso5Terminado = (ficha.Paso5 != null) && (ficha.Paso5.IdEstado == idEstadoTerminado);

            ViewBag.Paso5Terminado = paso5Terminado;



            // Crear Paso6 si no existe
            if (ficha.Paso6 == null)
            {
                var fechaEstimada = ficha.Paso5?.FechaFin?.AddDays(7) ?? DateTime.Now.AddDays(7);
                ficha.Paso6 = new Paso6
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = DateTime.Now,
                    FechaFinEstimada = fechaEstimada
                };
            }

            // Usuario actual
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Unauthorized();

            var rol = ficha.Equipo?.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)?
                .RolEnEquipo.ToLower() ?? "";

            ViewBag.EsPiloto = rol == "piloto";
            ViewBag.EsAuditor = rol == "auditor";
            ViewBag.EsResponsable = ficha.Paso6.Responsable == usuario.Id;
            ViewBag.EsSupervisor = usuario.Rol.Equals("supervisor", StringComparison.OrdinalIgnoreCase);

            // Dropdowns
            ViewBag.Estados6 = new SelectList(await _context.Estados.ToListAsync(), "IdEstado", "Descripcion", ficha.Paso6.IdEstado);

            var responsables = ficha.Equipo?.EquiposUsuarios
                .Where(eu => !eu.RolEnEquipo.Equals("piloto", StringComparison.OrdinalIgnoreCase))
                .Select(eu => eu.Usuario)
                .ToList() ?? new();
            ViewBag.Responsables6 = new SelectList(responsables, "Id", "NombreUsuario", ficha.Paso6.Responsable);

            ViewBag.Verificaciones = await _context.VerificacionesAcciones
                .Include(v => v.Tarea)
                 .Where(v => v.Tarea.IdFichaTecnica == idFicha)
                .ToListAsync();

            // 1) Obtengo los IdTarea ya verificados para esta ficha
            var idsVerificados = await (
                from v in _context.VerificacionesAcciones
                join t in _context.Tareas on v.IdTarea equals t.IdTarea
                where t.IdFichaTecnica == idFicha
                select v.IdTarea
            ).ToListAsync();

            // 2) Traigo sólo las tareas de mejora de la ficha que NO estén en idsVerificados
            var tareasDisponibles = await _context.Tareas
                .Where(t =>
                    t.IdFichaTecnica == idFicha &&
                    !string.IsNullOrEmpty(t.AccionDeMejora) &&
                    !idsVerificados.Contains(t.IdTarea)
                )
                .ToListAsync();

            ViewBag.AccionesMejora = new SelectList(
                tareasDisponibles,
                "IdTarea",
                "AccionDeMejora"
                 );


            return View("~/Views/FichasTecnicas/Paso6.cshtml", ficha);
        }

        [HttpGet("ObtenerAcciones")]
        public async Task<IActionResult> ObtenerAccionesDeMejoraPorFicha(int idFicha)
        {
            // 1) IDs de tareas ya verificadas en esta ficha
            var idsVerificados = await (
                from v in _context.VerificacionesAcciones
                join t in _context.Tareas on v.IdTarea equals t.IdTarea
                where t.IdFichaTecnica == idFicha
                select v.IdTarea
            ).ToListAsync();

            // 2) Sólo devuelvo las tareas de mejora pendientes
            var acciones = await _context.Tareas
                .Where(t =>
                    t.IdFichaTecnica == idFicha &&
                    !string.IsNullOrEmpty(t.AccionDeMejora) &&
                    !idsVerificados.Contains(t.IdTarea)
                )
                .Select(t => new {
                    Id = t.IdTarea,
                    Texto = t.AccionDeMejora
                })
                .ToListAsync();

            return Json(acciones);


            return Json(acciones);
        }

        // GET: /FichasTecnicas/{idFicha}/Paso6/ObtenerCorrida
        [HttpGet("ObtenerCorrida")]
        public async Task<IActionResult> ObtenerCorrida(int idFicha)
        {
            var corrida = await _context.CorridasProduccion
                .Where(c => c.IdFichaTecnica == idFicha)
                .OrderByDescending(c => c.IdProduccion)
                .FirstOrDefaultAsync();

            if (corrida == null)
                return NotFound();

            return Ok(new
            {
                idProduccion = corrida.IdProduccion,            
                fecha = corrida.FechaProduccion.ToString("yyyy-MM-dd"),
                producida = corrida.CantidadProducida,
                ok = corrida.CantidadOK,
                noOK = corrida.CantidadNoOK
            });
        }



        // POST: FichasTecnicas/{idFicha}/Paso6/Guardar
        [HttpPost("Guardar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(
            int idFicha,
            [Bind("FechaInicio,FechaFin,FechaFinEstimada,Responsable,IdEstado", Prefix = "Paso6")] Paso6 paso6Model)
        {
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso6)
                .Include(f => f.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Unauthorized();

            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)?
                .RolEnEquipo.ToLower() ?? "";

            int? estadoAnterior = ficha.Paso6?.IdEstado;

            if (ficha.Paso6 == null)
            {
                ficha.Paso6 = new Paso6
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now
                };
                _context.Paso6.Add(ficha.Paso6);
            }

            // Si el auditor intenta guardar, no puede cambiar el responsable
            if (rol == "auditor")
                paso6Model.Responsable = ficha.Paso6.Responsable;

            // Asignación de valores
            ficha.Paso6.FechaInicio = paso6Model.FechaInicio;
            ficha.Paso6.FechaFin = paso6Model.FechaFin;
            ficha.Paso6.FechaFinEstimada = paso6Model.FechaFinEstimada;
            ficha.Paso6.Responsable = paso6Model.Responsable;
            ficha.Paso6.IdEstado = paso6Model.IdEstado;

            await _context.SaveChangesAsync();

            // Notificaciones
            var estadoCompletado = await _context.Estados.FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var estadoTerminado = await _context.Estados.FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            var piloto = ficha.Equipo.EquiposUsuarios.FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")?.Usuario;

            if (estadoCompletado != null &&
                ficha.Paso6.IdEstado == estadoCompletado.IdEstado &&
                estadoAnterior != estadoCompletado.IdEstado &&
                piloto != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"El responsable completó el Paso 6 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = piloto.Id
                });
                await _context.SaveChangesAsync();
            }

            if (estadoTerminado != null &&
                ficha.Paso6.IdEstado == estadoTerminado.IdEstado &&
                estadoAnterior != estadoTerminado.IdEstado)
            {
                var auditores = ficha.Equipo.EquiposUsuarios
                    .Where(eu => eu.RolEnEquipo.ToLower() == "auditor" &&
                                 eu.Usuario.Id != piloto?.Id)
                    .Select(eu => eu.Usuario)
                    .ToList();

                foreach (var auditor in auditores)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        Mensaje = $"Paso 6 de la ficha técnica {idFicha} está terminado. Ya podés dar seguimiento final.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = auditor.Id
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });
        }


        private int ObtenerIdFichaDesdeTarea(int idTarea)
        {
            return _context.Tareas
                .Where(t => t.IdTarea == idTarea)
                .Select(t => t.IdFichaTecnica)
                .FirstOrDefault();
        }

        [HttpPost("GuardarVerificacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarVerificacion(VerificacionAccion verificacion)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(string.Join(" | ", errores));
            }


            // Guardar en base de datos
            _context.VerificacionesAcciones.Add(verificacion);
            await _context.SaveChangesAsync();

            // Redirigir a donde quieras (por ejemplo, al paso actual de la ficha técnica)
            return RedirectToAction("Index", new { idFicha = ObtenerIdFichaDesdeTarea(verificacion.IdTarea) });
        }

        [HttpPost("EditarVerificacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarVerificacion(int idFicha, VerificacionAccion verificacion)
        {
            if (!ModelState.IsValid)
            {
                // Podés devolver BadRequest o recargar la vista con errores
                return BadRequest(ModelState);
            }

            // 1) Traer la verificación existente
            var existente = await _context.VerificacionesAcciones
                                         .FindAsync(verificacion.IdVerificacion);
            if (existente == null)
                return NotFound();

            // 2) Asignar los nuevos valores
            existente.MetodoConfirmacion = verificacion.MetodoConfirmacion;
            existente.MetodoConfirmacion = verificacion.MetodoConfirmacion;
            existente.FechaVerificacion = verificacion.FechaVerificacion;

            // 3) Actualizar y guardar cambios
            _context.VerificacionesAcciones.Update(existente);
            await _context.SaveChangesAsync();

            // 4) Redirigir de vuelta al Paso6 de la ficha
            return RedirectToAction("Index", new { idFicha });
        }


        [HttpPost("GuardarCorrida")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCorrida(
            int idFicha,
            [Bind("IdFichaTecnica,CantidadProducida,CantidadOK,CantidadNoOK,FechaProduccion")]
    CorridaProduccion corrida)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            corrida.IdFichaTecnica = idFicha;
            _context.CorridasProduccion.Add(corrida);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { idFicha });
        }

        [HttpPost("EliminarVerificacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarVerificacion(int idFicha, int idVerificacion)
        {
            var entidad = await _context.VerificacionesAcciones.FindAsync(idVerificacion);
            if (entidad != null)
            {
                _context.VerificacionesAcciones.Remove(entidad);
                await _context.SaveChangesAsync();
            }
            // Redirigir de vuelta al listado del Paso6
            return RedirectToAction("Index", new { idFicha });
        }

        [HttpPost("EliminarCorrida")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCorrida(int idFicha, [FromForm] int idCorrida)
        {
            // 1) Intento por PK
            var corrida = await _context.CorridasProduccion.FindAsync(idCorrida);

            // 2) Si no encontró por PK, intento por columnas (por si la PK no es la esperada)
            if (corrida == null)
            {
                corrida = await _context.CorridasProduccion
                    .FirstOrDefaultAsync(x => x.IdProduccion == idCorrida || x.IdProduccion == idCorrida);
            }

            if (corrida != null)
            {
                _context.CorridasProduccion.Remove(corrida);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Corrida eliminada.";
            }
            else
            {
                TempData["Warn"] = $"No se encontró la corrida (id={idCorrida}).";
            }

            return RedirectToAction("Index", new { idFicha });
        }

        [HttpPost("EditarCorrida")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCorrida(
            int idFicha,
            int IdProduccion,
            int CantidadProducida,
            int CantidadOk,
            DateTime FechaProduccion)
        {
            var corrida = await _context.CorridasProduccion.FirstOrDefaultAsync(c =>
                c.IdProduccion == IdProduccion && c.IdFichaTecnica == idFicha);

            if (corrida == null)
            {
                // si no la encontró, opcionalmente podés crearla o devolver error
                TempData["Warn"] = "No se encontró la corrida a actualizar.";
                return RedirectToAction("Index", new { idFicha });
            }

            corrida.CantidadProducida = CantidadProducida;
            corrida.CantidadOK = CantidadOk;
            corrida.CantidadNoOK = Math.Max(CantidadProducida - CantidadOk, 0);
            corrida.FechaProduccion = FechaProduccion;

            await _context.SaveChangesAsync();
            TempData["Ok"] = "Corrida actualizada.";
            return RedirectToAction("Index", new { idFicha });
        }

    }
}
