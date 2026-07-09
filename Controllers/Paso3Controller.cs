using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("FichasTecnicas/{idFicha}/Paso3")]
    public class Paso3Controller : Controller
    {
        private readonly TesisDbContext _context;
        public Paso3Controller(TesisDbContext context) => _context = context;

        // GET: carga o inicializa Paso3, prepara permisos y dropdowns
        [HttpGet("")]
        public async Task<IActionResult> Index(int idFicha)
        {
            // 1) Cargo ficha con Paso3, Paso2 y Equipo/Usuarios
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso3)
                .Include(f => f.Paso2AnalisisCausa)
                .Include(f => f.Equipo)
                   .ThenInclude(e => e.EquiposUsuarios)
                     .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null)
                return NotFound();

            // 2) Si no existe Paso3, preparo uno en memoria con los valores por defecto
            if (ficha.Paso3 == null)
            {
                var paso2 = ficha.Paso2AnalisisCausa;
                var fechaFinEstim = paso2?.FechaFin?.AddDays(7)
                                   ?? DateTime.Now.AddDays(7);

                // Lo creo **en memoria**; NO lo guardo aquí
                ficha.Paso3 = new Paso3
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = DateTime.Now,
                    FechaFinEstimada = fechaFinEstim
                    // Responsable e IdEstado quedan para que el usuario los elija
                };
            }

            // Estado del Paso 2
            var estadoTerminado = await _context.Estados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            ViewBag.Paso2Terminado = ficha.Paso2AnalisisCausa != null
                && estadoTerminado != null
                && ficha.Paso2AnalisisCausa.IdEstado == estadoTerminado.IdEstado;

            // 3) Usuario actual y rol en el equipo
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null)
                return Unauthorized();

            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?
                .RolEnEquipo.ToLower() ?? "";

            ViewBag.EsPiloto = rol == "piloto";
            ViewBag.EsAuditor = rol == "auditor";
            ViewBag.EsResponsable = ficha.Paso3.Responsable == usuarioActual.Id;
            ViewBag.EsSupervisor = usuarioActual.Rol.Equals("supervisor", StringComparison.OrdinalIgnoreCase);

            // 4) Dropdown de Estados
            ViewBag.Estados3 = new SelectList(
                await _context.Estados.ToListAsync(),
                "IdEstado", "Descripcion",
                ficha.Paso3.IdEstado
            );

            // 5) Dropdown de Responsables (todos menos pilotos)
            var responsables = ficha.Equipo.EquiposUsuarios
                .Where(eu => !eu.RolEnEquipo.Equals("Piloto", StringComparison.OrdinalIgnoreCase))
                .Select(eu => eu.Usuario)
                .ToList();
            ViewBag.Responsables3 = new SelectList(
                responsables, "Id", "NombreUsuario",
                ficha.Paso3.Responsable
            );

            // 6) Datos para el modal de 5-porqués
            var analizados = await _context.Analisis5Porques
                .Where(a => a.IdFichaTecnica == idFicha)
                .Select(a => a.IdCausa)
                .ToListAsync();
            ViewBag.CausasFenomeno = await _context.CausasPasos
                .Where(cp => cp.IdFichaTecnica == idFicha && !analizados.Contains(cp.IdCausa))
                .Select(cp => new SelectListItem
                {
                    Value = cp.IdCausa.ToString(),
                    Text = cp.DescripcionCausa
                }).ToListAsync();
            ViewBag.Analisis5 = await _context.Analisis5Porques
                .Where(a => a.IdFichaTecnica == idFicha)
                .Include(a => a.CausaPaso)
                .ToListAsync();

            return View("~/Views/FichasTecnicas/Paso3.cshtml", ficha);
        }


        // POST: guarda fechas, responsable y estado en Paso3
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            int idFicha,
            [Bind("FechaInicio,FechaFinEstimada,FechaFin,Responsable,IdEstado", Prefix = "Paso3")]
    Paso3 paso3Model)
        {
            // 1) Cargo ficha con Paso3 y Equipo
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Paso3)
                .Include(f => f.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            // 2) Usuario actual
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null) return Unauthorized();

            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)
                ?.RolEnEquipo.ToLower() ?? "";

            // 3) Guardamos estado anterior
            int? estadoAnterior = ficha.Paso3?.IdEstado;

            // 4) Si no existía Paso3, lo inicializo (igual que en el GET)
            if (ficha.Paso3 == null)
            {
                ficha.Paso3 = new Paso3
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now
                };
                _context.Paso3.Add(ficha.Paso3);
            }

            // 5) Si es auditor, no puede cambiar responsable
            if (rol == "auditor")
                paso3Model.Responsable = ficha.Paso3.Responsable;

            // 6) Asignar valores del formulario
            ficha.Paso3.FechaInicio = paso3Model.FechaInicio;
            ficha.Paso3.FechaFinEstimada = paso3Model.FechaFinEstimada;
            ficha.Paso3.FechaFin = paso3Model.FechaFin;
            ficha.Paso3.Responsable = paso3Model.Responsable;
            ficha.Paso3.IdEstado = paso3Model.IdEstado;

            await _context.SaveChangesAsync(); // primero guardamos

            // 7) Notificaciones por estado
            var estadoCompletado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var estadoTerminado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            var piloto = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")
                ?.Usuario;

            // COMPLETADO → Notificar al piloto
            if (estadoCompletado != null &&
                ficha.Paso3.IdEstado == estadoCompletado.IdEstado &&
                estadoAnterior != estadoCompletado.IdEstado &&
                piloto != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"El responsable completó el Paso 3 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = piloto.Id
                });
                await _context.SaveChangesAsync();
            }

            // TERMINADO → Notificar a los auditores (excepto al piloto si lo es)
            if (estadoTerminado != null &&
                ficha.Paso3.IdEstado == estadoTerminado.IdEstado &&
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
                        Mensaje = $"Paso 3 de la ficha técnica {idFicha} está terminado. Ya está habilitado el Paso 4.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = auditor.Id
                    });
                }

                await _context.SaveChangesAsync();
            }

            // 8) Redirigir a la vista de detalle
            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });
        }



        // POST: crea/edita un análisis de 5-porqués
        // POST: crea/edita un análisis de 5-porqués
        [HttpPost("CrearAnalisis")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAnalisis(
            int idFicha,                         // viene de la ruta
            [FromForm] int IdCausa,
            [FromForm] string PrimerPorque,
            [FromForm] string SegundoPorque,
            [FromForm] string TercerPorque,
            [FromForm] string CuartoPorque,
            [FromForm] string QuintoPorque,
            [FromForm] bool EsCausaRaiz,
            [FromForm] int? IdAnalisis)
        {
            // 1) Cargo ficha y equipo con usuarios
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            // 2) Usuario actual y rol en equipo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Unauthorized();
            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)?
                .RolEnEquipo.ToLower() ?? "";

       

            // 4) Crear o actualizar el análisis
            Analisis5Porque analisis;
            if (IdAnalisis.HasValue && IdAnalisis.Value > 0)
            {
                analisis = await _context.Analisis5Porques.FindAsync(IdAnalisis.Value);
                if (analisis == null) return NotFound();
            }
            else
            {
                analisis = new Analisis5Porque { IdFichaTecnica = idFicha, IdCausa = IdCausa };
                _context.Analisis5Porques.Add(analisis);
            }

            analisis.PrimerPorque = PrimerPorque;
            analisis.SegundoPorque = SegundoPorque;
            analisis.TercerPorque = TercerPorque;
            analisis.CuartoPorque = CuartoPorque;
            analisis.QuintoPorque = QuintoPorque;

            // 5) Marcar causa raíz en la entidad CausasPasos
            var causa = await _context.CausasPasos.FirstOrDefaultAsync(cp =>
                cp.IdFichaTecnica == idFicha &&
                cp.IdCausa == IdCausa);
            if (causa != null)
                causa.EsCausaRaiz = EsCausaRaiz ? (short)1 : (short)0;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { idFicha });
        }

        // POST: elimina un análisis y resetea la causa raíz
        [HttpPost("EliminarAnalisis")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAnalisis(
            int idFicha,                   // viene de la ruta
            [FromForm] int IdAnalisis)
        {
            // 1) Cargo ficha y equipo para permisos
            var ficha = await _context.FichasTecnicas
                .Include(f => f.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(f => f.IdFichaTecnica == idFicha);
            if (ficha == null) return NotFound();

            // 2) Usuario actual y rol
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return Unauthorized();
            var rol = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)?
                .RolEnEquipo.ToLower() ?? "";

     

            // 4) Eliminar análisis y resetear marca de causa raíz
            var analisis = await _context.Analisis5Porques.FindAsync(IdAnalisis);
            if (analisis == null) return NotFound();

            var causa = await _context.CausasPasos.FirstOrDefaultAsync(cp =>
                cp.IdFichaTecnica == idFicha &&
                cp.IdCausa == analisis.IdCausa);
            if (causa != null)
                causa.EsCausaRaiz = 0;

            _context.Analisis5Porques.Remove(analisis);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { idFicha });
        }

    }
}
