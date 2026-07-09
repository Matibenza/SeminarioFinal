using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("FichasTecnicas/{idFicha}/Paso5")]
    public class Paso5Controller : Controller
    {
        private readonly TesisDbContext _context;

        public Paso5Controller(TesisDbContext context)
        {
            _context = context;
        }

        // GET: FichasTecnicas/{idFicha}/Paso5
        [HttpGet("")]
        public async Task<IActionResult> Index(int idFicha)
        {
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso5)
                .Include(f => f.Paso3) // opcional si usás fechaFinEstimada
                .Include(f => f.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            // Crear Paso5 si no existe
            if (ficha.Paso5 == null)
            {
                var fechaEstimada = ficha.Paso3?.FechaFin?.AddDays(7) ?? DateTime.Now.AddDays(7);
                ficha.Paso5 = new Paso5
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

            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)?
                .RolEnEquipo.ToLower() ?? "";

            ViewBag.EsPiloto = rol == "piloto";
            ViewBag.EsAuditor = rol == "auditor";
            ViewBag.EsResponsable = ficha.Paso5.Responsable == usuario.Id;
            ViewBag.EsSupervisor = usuario.Rol.Equals("supervisor", StringComparison.OrdinalIgnoreCase);

            // Dropdowns
            ViewBag.Estados5 = new SelectList(await _context.Estados.ToListAsync(), "IdEstado", "Descripcion", ficha.Paso5.IdEstado);

            var responsables = ficha.Equipo.EquiposUsuarios
                .Where(eu => !eu.RolEnEquipo.Equals("piloto", StringComparison.OrdinalIgnoreCase))
                .Select(eu => eu.Usuario)
                .ToList();
            ViewBag.Responsables5 = new SelectList(responsables, "Id", "NombreUsuario", ficha.Paso5.Responsable);

            return View("~/Views/FichasTecnicas/Paso5.cshtml", ficha);
        }

        // POST: FichasTecnicas/{idFicha}/Paso5/Guardar
        [HttpPost("Guardar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(
     int idFicha,
     [Bind("FechaInicio,FechaFin,FechaFinEstimada,Responsable,IdEstado,FacilidadSeguimiento,Instrucciones,EstandarCalidad,Defectos,ParametrosCeroDefectos,Variacion,VariacionTrabajo,LiberacionDefectos", Prefix = "Paso5")] Paso5 paso5Model)
        {
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso5)
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

            int? estadoAnterior = ficha.Paso5?.IdEstado;

            if (ficha.Paso5 == null)
            {
                ficha.Paso5 = new Paso5
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now
                };
                _context.Paso5.Add(ficha.Paso5);
            }

            if (rol == "auditor")
                paso5Model.Responsable = ficha.Paso5.Responsable;



            // Asignación de valores mapeados
            ficha.Paso5.FechaInicio = paso5Model.FechaInicio;
            ficha.Paso5.FechaFin = paso5Model.FechaFin;
            ficha.Paso5.FechaFinEstimada = paso5Model.FechaFinEstimada;
            ficha.Paso5.Responsable = paso5Model.Responsable;
            ficha.Paso5.IdEstado = paso5Model.IdEstado;

            ficha.Paso5.FacilidadSeguimiento = paso5Model.FacilidadSeguimiento;
            ficha.Paso5.Instrucciones = paso5Model.Instrucciones;
            ficha.Paso5.EstandarCalidad = paso5Model.EstandarCalidad;
            ficha.Paso5.Defectos = paso5Model.Defectos;
            ficha.Paso5.ParametrosCeroDefectos = paso5Model.ParametrosCeroDefectos;
            ficha.Paso5.Variacion = paso5Model.Variacion;
            ficha.Paso5.VariacionTrabajo = paso5Model.VariacionTrabajo;
            ficha.Paso5.LiberacionDefectos = paso5Model.LiberacionDefectos;



            await _context.SaveChangesAsync();

            // Notificaciones
            var estadoCompletado = await _context.Estados.FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var estadoTerminado = await _context.Estados.FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            var piloto = ficha.Equipo.EquiposUsuarios.FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")?.Usuario;

            if (estadoCompletado != null &&
                ficha.Paso5.IdEstado == estadoCompletado.IdEstado &&
                estadoAnterior != estadoCompletado.IdEstado &&
                piloto != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"El responsable completó el Paso 5 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = piloto.Id
                });
                await _context.SaveChangesAsync();
            }

            if (estadoTerminado != null &&
                ficha.Paso5.IdEstado == estadoTerminado.IdEstado &&
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
                        Mensaje = $"Paso 5 de la ficha técnica {idFicha} está terminado. Ya está habilitado el Paso 6.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = auditor.Id
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });
        }

    }
}
