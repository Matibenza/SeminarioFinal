using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("FichasTecnicas/{idFicha}/Paso1")]
    public class Paso1Controller : Controller
    {
        private readonly TesisDbContext _context;

        public Paso1Controller(TesisDbContext context)
        {
            _context = context;
        }

        // GET: FichasTecnicas/{idFicha}/Paso1
        [HttpGet]
        public async Task<IActionResult> Index(int idFicha)
        {
            // ─── 1) Cargo la ficha técnica con su equipo ─── 
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);
            if (ficha == null)
                return NotFound();

            // ─── 2) Usuario actual y su rol en el equipo ───
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null)
                return Unauthorized();

            var rolUsuarioEnEquipo = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?
                .RolEnEquipo.ToLower() ?? "";

            // ─── 3) Intento cargar Paso 0 para tomar FechaFin ───
            var paso0 = await _context.Paso0Contenciones
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);

            // ─── 3.1) ¿Paso 0 terminado? ───
            var estadoTerminado = await _context.Estados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");
            ViewBag.Paso0Terminado = paso0 != null
                && estadoTerminado != null
                && paso0.IdEstado == estadoTerminado.IdEstado;

            // ─── 4) Intento cargar Paso 1 ───
            var paso1 = await _context.Paso1Verificaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);

            // ─── 5) Inicializo Paso 1 si no existe ───
            if (paso1 == null)
            {
                paso1 = new Paso1Verificacion
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = DateTime.Now,
                    // FechaFinEstimada = FechaFin de Paso0 + 7 días (o ahora +7d si no hay Paso0)
                    FechaFinEstimada = paso0?.FechaFin?.AddDays(7)
                                         ?? DateTime.Now.AddDays(7),
                    // Por defecto pongo responsable al usuario actual
                    Responsable = usuarioActual.Id,
                    // Y estado inicial tomo el mismo que Paso0, o 1 si no hay Paso0
                    IdEstado = paso0?.IdEstado ?? 1
                };
            }

            // ─── 6) Si NO es piloto, forzar responsable al que haya quedado guardado ───
            if (rolUsuarioEnEquipo != "piloto" && usuarioActual.Rol.ToLower() != "supervisor")
            {
                var paso1db = await _context.Paso1Verificaciones
                    .FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);
                if (paso1db != null)
                    paso1.Responsable = paso1db.Responsable;
            }

            // ─── 7) Calcular flags de rol para la UI ───
            ViewBag.EsPiloto = rolUsuarioEnEquipo == "piloto";
            ViewBag.EsResponsable = paso1.Responsable == usuarioActual.Id;
            ViewBag.EsSupervisor = usuarioActual.Rol.ToLower() == "supervisor";
            ViewBag.EsAuditor = rolUsuarioEnEquipo == "auditor";

            // ─── 8) Poblar listas (empleados, auditores, estados) ───
            await PopulateSelectLists(idFicha);

            var responsableUsuario = await _context.Usuarios.FindAsync(paso1.Responsable);
            ViewBag.ResponsablePaso1 = responsableUsuario?.NombreUsuario ?? "-";

            // ─── 9) Devolver la vista Paso1.cshtml ───
            return View(
                "~/Views/FichasTecnicas/Paso1.cshtml",
                paso1
            );
        }


        // POST: FichasTecnicas/{idFicha}/Paso1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int idFicha, Paso1Verificacion model)
        {
            // 0) Cargo ficha con equipo y usuarios
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);
            if (ficha == null)
                return NotFound();

            // 1) Obtengo usuario actual y su rol en el equipo
            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null)
                return Unauthorized();
            var rolUsuarioEnEquipo = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuarioActual.Id)?
                .RolEnEquipo.ToLower() ?? "";

            // 2) Repoblo dropdowns en ViewBag
            await PopulateSelectLists(idFicha);

            // 3) Si NO es piloto, fuerzo el Responsable al guardado previamente
            if (rolUsuarioEnEquipo != "piloto" && usuarioActual.Rol.ToLower() != "supervisor")
            {
                var pasoDb = await _context.Paso1Verificaciones
                    .FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);
                if (pasoDb != null)
                    model.Responsable = pasoDb.Responsable;
            }

            // 4) Validación básica de modelo
            if (!ModelState.IsValid)
            {
                SetearPermisosPaso1(ViewBag, model, usuarioActual, rolUsuarioEnEquipo);
                return View("~/Views/FichasTecnicas/Paso1.cshtml", model);
            }

            // 5) Validar que el Responsable exista
            var responsableExiste = await _context.Usuarios
                .AnyAsync(u => u.Id == model.Responsable);
            if (!responsableExiste)
            {
                ModelState.AddModelError("", "El usuario responsable seleccionado no existe.");
                SetearPermisosPaso1(ViewBag, model, usuarioActual, rolUsuarioEnEquipo);
                return View("~/Views/FichasTecnicas/Paso1.cshtml", model);
            }

            // 6) Preparo el model para guardado
            model.IdFichaTecnica = idFicha;

            // 7) Obtengo existente y guardo estado anterior
            var existente = await _context.Paso1Verificaciones.FindAsync(idFicha);
            int? estadoAnterior = null;
            if (existente != null)
            {
                estadoAnterior = existente.IdEstado;

                // Actualizo sólo los campos de Paso 1
                existente.FechaInicio = model.FechaInicio;
                existente.FechaFin = model.FechaFin;
                existente.FechaFinEstimada = model.FechaFinEstimada;
                existente.Responsable = model.Responsable;
                existente.IdEstado = model.IdEstado;
                existente.TipoProblema = model.TipoProblema;
                existente.IdOperador = model.IdOperador;
                existente.Turno = model.Turno;
                existente.Objetivo = model.Objetivo;
                existente.QueSucede = model.QueSucede;
                existente.QuienDetecta = model.QuienDetecta;
                existente.CuandoSucede = model.CuandoSucede;
                existente.ComoSucede = model.ComoSucede;
                existente.CualPieza = model.CualPieza;
            }
            else
            {
                _context.Paso1Verificaciones.Add(model);
            }

            await _context.SaveChangesAsync();

            // 8) Notificaciones en base al cambio de estado
            var estadoCompletado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var estadoTerminado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            // Piloto del equipo
            var piloto = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")
                ?.Usuario;

            // Notificar a piloto cuando pase a COMPLETADO
            if (estadoCompletado != null
                && model.IdEstado == estadoCompletado.IdEstado
                && estadoAnterior != estadoCompletado.IdEstado
                && piloto != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"El responsable completó el Paso 1 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = piloto.Id
                });
                await _context.SaveChangesAsync();
            }

            // Notificar a auditores (menos piloto) cuando pase a TERMINADO
            if (estadoTerminado != null
                && model.IdEstado == estadoTerminado.IdEstado
                && estadoAnterior != estadoTerminado.IdEstado)
            {
                var auditores = ficha.Equipo.EquiposUsuarios
                    .Where(eu => eu.RolEnEquipo.ToLower() == "auditor"
                              && eu.Usuario.Id != piloto?.Id)
                    .Select(eu => eu.Usuario)
                    .ToList();

                foreach (var aud in auditores)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        Mensaje = $"Paso 1 de la ficha técnica {idFicha} está terminado. Ya está habilitado el Paso 2.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = aud.Id
                    });
                }
                await _context.SaveChangesAsync();
            }

            // 9) Redirigir a la vista de detalle (o Paso2 según tu flujo)
            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });
        }

        // Helper para setear flags en ViewBag (idéntico a Paso0)
        private void SetearPermisosPaso1(dynamic ViewBag, Paso1Verificacion paso, Usuario usuarioActual, string rolUsuarioEnEquipo)
        {
            ViewBag.EsAuditor = rolUsuarioEnEquipo == "auditor";
            ViewBag.EsPiloto = rolUsuarioEnEquipo == "piloto";
            ViewBag.EsResponsable = paso.Responsable == usuarioActual.Id;
            ViewBag.EsSupervisor = usuarioActual.Rol.ToLower() == "supervisor";
        }

        // llena Empleados, Auditores y Estados
        private async Task PopulateSelectLists(int idFicha)
        {
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);

            var empleados = await _context.Usuarios
                .Where(u => u.Rol == "Empleado")
                .ToListAsync();

            var auditores = ficha.Equipo.EquiposUsuarios
                .Where(eu => eu.RolEnEquipo == "Auditor")
                .Select(eu => eu.Usuario)
                .ToList();

            var estados = await _context.Estados.ToListAsync();
            var miembros = ficha.Equipo.EquiposUsuarios
                 .Where(eu => eu.RolEnEquipo.ToLower() != "piloto")
                .Select(eu => eu.Usuario)
                .ToList();

            ViewBag.Usuarios = new SelectList(miembros, "Id", "NombreUsuario");
            ViewBag.Empleados = new SelectList(empleados, "Id", "NombreUsuario");
            ViewBag.Auditores = new SelectList(auditores, "Id", "NombreUsuario");
            ViewBag.Estados = new SelectList(estados, "IdEstado", "Descripcion");
        }

    }
}
