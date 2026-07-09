using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("FichasTecnicas/{idFicha}/Paso0")]
    public class Paso0Controller : Controller
    {
        private readonly TesisDbContext _context;
        public Paso0Controller(TesisDbContext context)
        {
            _context = context;
        }

        // GET  /FichasTecnicas/{idFicha}/Paso0
        [HttpGet]
        public async Task<IActionResult> Index(int idFicha)
        {
            // 1) Cargar ficha técnica con su equipo (una sola vez)
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);

            if (ficha == null)
                return NotFound();

            // 2) Obtener usuario actual y su rol en el equipo
            var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            var rolUsuarioEnEquipo = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?.RolEnEquipo.ToLower() ?? "";

            // 3) Intentar cargar paso 0
            var paso = await _context.Paso0Contenciones
                                     .FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);

            if (paso == null)
            {
                // Inicializo el Paso 0 si no existe
                if (usuarioActual == null)
                    return Unauthorized();

                paso = new Paso0Contencion
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = DateTime.Now,
                    IdEstado = 1, // estado inicial
                    Responsable = usuarioActual.Id,
                    FechaFinEstimada = ficha.FechaCreacion.AddDays(4)  // +4 días
                };
            }

            // Si NO es piloto, forzá el responsable a lo que esté guardado en la BD (si lo hay)
            if (rolUsuarioEnEquipo != "piloto")
            {
                var pasoBD = await _context.Paso0Contenciones.FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);
                if (pasoBD != null)
                    paso.Responsable = pasoBD.Responsable;
            }

            bool esPiloto = rolUsuarioEnEquipo == "piloto";
            bool esResponsable = paso.Responsable == usuarioActual.Id;
            bool esSupervisor = usuarioActual.Rol.ToLower() == "supervisor";

            ViewBag.EsPiloto = esPiloto;
            ViewBag.EsResponsable = esResponsable;
            ViewBag.EsSupervisor = esSupervisor;
            ViewBag.EsAuditor = rolUsuarioEnEquipo == "auditor";

            // Poner en ViewBag lista de responsables
            var miembros = ficha.Equipo.EquiposUsuarios
                .Where(eu => eu.RolEnEquipo.ToLower() != "piloto")
                .Select(eu => eu.Usuario)
                .ToList();

            ViewBag.Usuarios = new SelectList(miembros, "Id", "NombreUsuario", paso.Responsable);

            // Poner en ViewBag lista de estados
            var estados = await _context.Estados.ToListAsync();
            ViewBag.Estados = new SelectList(estados, "IdEstado", "Descripcion", paso.IdEstado);

            // Calcular tiempo restante
            TimeSpan restanteTs = paso.FechaFinEstimada.HasValue
                ? paso.FechaFinEstimada.Value - DateTime.Now
                : TimeSpan.Zero;

            string restanteTexto;
            if (restanteTs <= TimeSpan.Zero)
                restanteTexto = "0 H";
            else if (restanteTs.TotalDays >= 1)
                restanteTexto = $"{(int)restanteTs.TotalDays} día{((int)restanteTs.TotalDays == 1 ? "" : "s")}";
            else
                restanteTexto = $"{(int)restanteTs.TotalHours} H";

            ViewBag.TiempoRestante = restanteTexto;

            return View("~/Views/FichasTecnicas/Paso0.cshtml", paso);
        }


        // POST /FichasTecnicas/{idFicha}/Paso0
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int idFicha, Paso0Contencion model)
        {
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);
            if (ficha == null)
                return NotFound();

            var miembros = ficha.Equipo.EquiposUsuarios
                .Where(eu => eu.RolEnEquipo.ToLower() != "piloto")
                .Select(eu => eu.Usuario)
                .ToList();
            ViewBag.Usuarios = new SelectList(miembros, "Id", "NombreUsuario", model.Responsable);

            var estados = await _context.Estados.ToListAsync();
            ViewBag.Estados = new SelectList(estados, "IdEstado", "Descripcion", model.IdEstado);

            var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            var rolUsuarioEnEquipo = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?.RolEnEquipo.ToLower() ?? "";

            if (rolUsuarioEnEquipo != "piloto")
            {
                var pasoBD = await _context.Paso0Contenciones.FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);
                if (pasoBD != null)
                    model.Responsable = pasoBD.Responsable;
            }

            if (!ModelState.IsValid)
            {
                SetearPermisosPaso0(ViewBag, model, usuarioActual, rolUsuarioEnEquipo);
                return View("~/Views/FichasTecnicas/Paso0.cshtml", model);
            }

            var responsableExiste = await _context.Usuarios.AnyAsync(u => u.Id == model.Responsable);
            if (!responsableExiste)
            {
                ModelState.AddModelError("", "El usuario responsable seleccionado no existe.");
                SetearPermisosPaso0(ViewBag, model, usuarioActual, rolUsuarioEnEquipo);
                return View("~/Views/FichasTecnicas/Paso0.cshtml", model);
            }

            model.IdFichaTecnica = idFicha;

            var existente = await _context.Paso0Contenciones.FindAsync(idFicha);

            int? estadoAnteriorId = null;
            if (existente != null)
            {
                estadoAnteriorId = existente.IdEstado;

                existente.AccionContencion = model.AccionContencion;
                existente.MetodoControl = model.MetodoControl;
                existente.Deposito_CantSospechosa = model.Deposito_CantSospechosa;
                existente.Deposito_CantControlada = model.Deposito_CantControlada;
                existente.Deposito_CantOk = model.Deposito_CantOk;
                existente.Almacen_CantSospechosa = model.Almacen_CantSospechosa;
                existente.Almacen_CantControlada = model.Almacen_CantControlada;
                existente.Almacen_CantOk = model.Almacen_CantOk;
                existente.BordeLinea_CantSospechosa = model.BordeLinea_CantSospechosa;
                existente.BordeLinea_CantControlada = model.BordeLinea_CantControlada;
                existente.BordeLinea_CantOk = model.BordeLinea_CantOk;
                existente.FechaInicio = model.FechaInicio;
                existente.FechaFin = model.FechaFin;
                existente.IdEstado = model.IdEstado;
                existente.Responsable = model.Responsable;
                existente.FechaFinEstimada = model.FechaFinEstimada;
            }
            else
            {
                _context.Paso0Contenciones.Add(model);
            }

            await _context.SaveChangesAsync();

            // Obtener estados COMPLETADO y TERMINADO
            var estadoCompletado = await _context.Estados.FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var estadoTerminado = await _context.Estados.FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            var piloto = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")?.Usuario;

            // Notificación para COMPLETADO (solo al piloto)
            if (estadoCompletado != null
                && model.IdEstado == estadoCompletado.IdEstado
                && estadoAnteriorId != estadoCompletado.IdEstado
                && piloto != null)
            {
                var notificacion = new Notificacion
                {
                    Mensaje = $"El responsable ya completó el paso 0 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = piloto.Id
                };
                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();
            }

            // Notificación para TERMINADO (a todos los auditores menos piloto)
            if (estadoTerminado != null
                && model.IdEstado == estadoTerminado.IdEstado
                && estadoAnteriorId != estadoTerminado.IdEstado)
            {
                var auditores = ficha.Equipo.EquiposUsuarios
                    .Where(eu => eu.RolEnEquipo.ToLower() == "auditor" && eu.Usuario.Id != piloto?.Id)
                    .Select(eu => eu.Usuario)
                    .ToList();

                foreach (var auditor in auditores)
                {
                    var notificacionTerminado = new Notificacion
                    {
                        Mensaje = $"Paso 0 de la ficha técnica {idFicha} está terminado. Ya está habilitado el paso 1.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = auditor.Id
                    };
                    _context.Notificaciones.Add(notificacionTerminado);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });
        }


        // Método auxiliar para setear permisos en ViewBag
        private void SetearPermisosPaso0(dynamic ViewBag, Paso0Contencion paso, Usuario usuarioActual, string rolUsuarioEnEquipo)
        {
            ViewBag.EsAuditor = rolUsuarioEnEquipo == "auditor";
            ViewBag.EsPiloto = rolUsuarioEnEquipo == "piloto";
            ViewBag.EsResponsable = paso.Responsable == usuarioActual.Id;
            ViewBag.EsSupervisor = usuarioActual.Rol.ToLower() == "supervisor";
        }

    }
}
