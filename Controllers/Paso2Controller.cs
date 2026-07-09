using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("FichasTecnicas/{idFicha}/Paso2")]
    public class Paso2Controller : Controller
    {
        private readonly TesisDbContext _context;
        public Paso2Controller(TesisDbContext context) => _context = context;

        // GET: FichasTecnicas/{idFicha}/Paso2
        [HttpGet]
        public async Task<IActionResult> Index(int idFicha)
        {
            // 1) Cargo ficha con todos los Includes necesarios
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Paso1Verificacion)
                .Include(ft => ft.Paso0Contencion)
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)
                .Include(ft => ft.Estado)
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);
            if (ficha == null)
                return NotFound();

            // 2) Usuario actual y su rol
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Forbid();

            var usuario = await _context.Usuarios
                .Include(u => u.Equipos_Usuarios)
                .FirstOrDefaultAsync(u => u.NombreUsuario == userName);
            if (usuario == null)
                return Forbid();

            var rolEnEquipo = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)
                ?.RolEnEquipo?.ToLower() ?? "";

            // 3) Recupero o inicializo la entidad Paso2
            var paso2 = await _context.Paso2AnalisisCausas
                .FirstOrDefaultAsync(p => p.IdFichaTecnica == idFicha);
            bool esNuevo = paso2 == null;
            if (esNuevo)
            {
                // EXTRAIGO valores no-nullable de la relación Paso1Verificacion
                var paso1 = ficha.Paso1Verificacion;

                DateTime fechaFinEstim = paso1?.FechaFin?.AddDays(7)
                                           ?? DateTime.Now.AddDays(7);

                int estadoInicial = paso1?.IdEstado   
                                   ?? ficha.IdEstado   
                                   ?? 0;

                paso2 = new Paso2AnalisisCausa
                {
                    IdFichaTecnica = idFicha,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = DateTime.Now,
                    FechaFinEstimada = fechaFinEstim,
                    IdEstado = estadoInicial,
                    Responsable = usuario.Id
                };
            }

            // 4) Poblar selects para la vista
            await PopulateSelectLists(ficha, paso2, usuario);

            // 5) Causas ya guardadas
            ViewBag.CausasExistentes = await _context.CausasPasos
                .Include(cp => cp.Categoria5M)            
                .Where(cp => cp.IdFichaTecnica == idFicha)
                .ToListAsync();

            // 6) Flags de UI
            ViewBag.EsAuditor = rolEnEquipo == "auditor";
            ViewBag.EsPiloto = rolEnEquipo == "piloto";
            ViewBag.EsResponsable = paso2.Responsable == usuario.Id;
            ViewBag.EsSupervisor = usuario.Rol
                .Equals("supervisor", StringComparison.OrdinalIgnoreCase);
            ViewBag.Notificaciones = await _context.Notificaciones
                .Where(n => !n.Leida && n.UsuarioId == usuario.Id)
                .ToListAsync();
            ViewBag.EsNuevo = esNuevo;

            //7 Paso1Terminado = true si Paso1 tiene estado "Terminado"
            var estadoTerminado = await _context.Estados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            ViewBag.Paso1Terminado = ficha.Paso1Verificacion != null
                && estadoTerminado != null
                && ficha.Paso1Verificacion.IdEstado == estadoTerminado.IdEstado;

            // 7) Devuelvo la vista con el modelo
            return View("~/Views/FichasTecnicas/Paso2.cshtml", paso2);
        }



        // POST: FichasTecnicas/{idFicha}/Paso2
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int idFicha, Paso2AnalisisCausa model)
        {
            // 1) Cargo ficha CON Equipo, EquiposUsuarios Y Usuario
            var ficha = await _context.FichasTecnicas
                .Include(ft => ft.Equipo)
                    .ThenInclude(e => e.EquiposUsuarios)
                        .ThenInclude(eu => eu.Usuario)     
                .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == idFicha);
            if (ficha == null)
                return NotFound();

            // 2) Usuario actual
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Forbid();

            var usuario = await _context.Usuarios
                .Include(u => u.Equipos_Usuarios)
                .FirstOrDefaultAsync(u => u.NombreUsuario == userName);
            if (usuario == null)
                return Forbid();

            // 3) Forzo el FK y repoblo selects
            model.IdFichaTecnica = idFicha;
            await PopulateSelectLists(ficha, model, usuario);

            // 4) Si hay error de validación, devuelvo la vista con datos + causas
            if (!ModelState.IsValid)
            {
                // causas existentes
                ViewBag.CausasExistentes = await _context.CausasPasos
                    .Include(cp => cp.Categoria5M)
                    .Where(cp => cp.IdFichaTecnica == idFicha)
                    .ToListAsync();

                // calculo rolEnEquipo protegiendo contra nulls
                var equipoUsuarios = ficha.Equipo?.EquiposUsuarios
                                    ?? Enumerable.Empty<Equipos_Usuarios>();
                var rolEnEquipo = equipoUsuarios
                    .FirstOrDefault(eu => eu.IdUsuario == usuario.Id)
                    ?.RolEnEquipo?.ToLower()
                    ?? "";


                ViewBag.EsAuditor = rolEnEquipo == "auditor";
                ViewBag.EsPiloto = rolEnEquipo == "piloto";
                ViewBag.EsResponsable = model.Responsable == usuario.Id;
                ViewBag.EsSupervisor = usuario.Rol
                    .Equals("supervisor", StringComparison.OrdinalIgnoreCase);
                ViewBag.Notificaciones = await _context.Notificaciones
                    .Where(n => !n.Leida && n.UsuarioId == usuario.Id)
                    .ToListAsync();
                ViewBag.EsNuevo = !await _context.Paso2AnalisisCausas
                    .AnyAsync(p => p.IdFichaTecnica == idFicha);

                return View("~/Views/FichasTecnicas/Paso2.cshtml", model);
            }

            // 5) Guardar o actualizar la entidad
            // 5) Guardar o actualizar la entidad y obtener estado anterior
            int? estadoAnterior = null;
            var existente = await _context.Paso2AnalisisCausas.FindAsync(idFicha);
            if (existente != null)
            {
                estadoAnterior = existente.IdEstado;

                existente.FechaInicio = model.FechaInicio;
                existente.FechaFin = model.FechaFin;
                existente.FechaFinEstimada = model.FechaFinEstimada;
                existente.Responsable = model.Responsable;
                existente.IdEstado = model.IdEstado;
            }
            else
            {
                model.FechaCreacion = DateTime.Now;
                _context.Paso2AnalisisCausas.Add(model);
            }

            // 🧠 Guardamos antes de calcular notificaciones
            await _context.SaveChangesAsync();

            // 6) Notificaciones basadas en estado
            var estadoCompletado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "COMPLETADO");
            var estadoTerminado = await _context.Estados
                .FirstOrDefaultAsync(e => e.Descripcion.ToUpper() == "TERMINADO");

            // Piloto del equipo
            var piloto = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")
                ?.Usuario;

            // Notificación: COMPLETADO → piloto
            if (estadoCompletado != null &&
                model.IdEstado == estadoCompletado.IdEstado &&
                estadoAnterior != estadoCompletado.IdEstado &&
                piloto != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"El responsable completó el Paso 2 de la ficha técnica {idFicha}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now,
                    UsuarioId = piloto.Id
                });
                await _context.SaveChangesAsync();
            }

            // Notificación: TERMINADO → auditores
            if (estadoTerminado != null &&
                model.IdEstado == estadoTerminado.IdEstado &&
                estadoAnterior != estadoTerminado.IdEstado)
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
                        Mensaje = $"Paso 2 de la ficha técnica {idFicha} está terminado. Ya está habilitado el Paso 3.",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        UsuarioId = aud.Id
                    });
                }
                await _context.SaveChangesAsync();
            }

            // 7) Redirigimos
            return RedirectToAction("Detalle", "FichasTecnicas", new { id = idFicha });

        }


        // POST: FichasTecnicas/{idFicha}/Paso2/CrearCausa
        [HttpPost("CrearCausa"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCausa(
     int idFicha,
     [Bind(Prefix = "NuevaCausa")] CausaPaso causaModel)
        {
            // 0) Verificar que ya exista Paso2 para esta ficha
            bool paso2Exists = await _context.Paso2AnalisisCausas
                .AsNoTracking()
                .AnyAsync(p => p.IdFichaTecnica == idFicha);

            if (!paso2Exists)
            {
                ModelState.AddModelError("",
                    "Primero debes guardar los datos del Paso 2 (Responsable y Estado) antes de agregar causas.");
                // Reutilizamos la acción GET Index para devolver la vista con todo lo necesario
                return await Index(idFicha);
            }

            // 1) Validación del modelo
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .SelectMany(m => m.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(string.Join("; ", errores));
            }

            // 2) Asignar la FK
            causaModel.IdFichaTecnica = idFicha;

            // 3) Editar existente o crear nueva
            if (causaModel.IdCausa > 0)
            {
                var existente = await _context.CausasPasos
                    .FindAsync(idFicha, causaModel.IdCausa);
                if (existente == null)
                    return NotFound();

                existente.IdCategoria5M = causaModel.IdCategoria5M;
                existente.DescripcionCausa = causaModel.DescripcionCausa;
                existente.ClasificacionImpacto = causaModel.ClasificacionImpacto;
                existente.ResultadoVerificacion = causaModel.ResultadoVerificacion;

                await _context.SaveChangesAsync();
            }
            else
            {
                var maxId = await _context.CausasPasos
                    .Where(c => c.IdFichaTecnica == idFicha)
                    .Select(c => (int?)c.IdCausa)
                    .MaxAsync() ?? 0;
                causaModel.IdCausa = maxId + 1;
                _context.CausasPasos.Add(causaModel);
                await _context.SaveChangesAsync();
            }

            // 4) Volver al índice
            return RedirectToAction(nameof(Index), new { idFicha });
        }


        /// <summary>
        /// Rellena ViewBag.Estados, ViewBag.Responsables y ViewBag.Categorias5M
        /// </summary>
        private async Task PopulateSelectLists(
            FichaTecnica ficha,
            Paso2AnalisisCausa paso2,
            Usuario usuario)
        {
            // 1) Estados
            var estados = await _context.Estados.ToListAsync();
            ViewBag.Estados = new SelectList(
                estados, "IdEstado", "Descripcion", paso2.IdEstado);

            // 2) Obtener siempre la colección de Equipos_Usuarios o lista vacía
            //    Fíjate que la propiedad en Equipo es ICollection<Equipos_Usuarios> EquiposUsuarios
            var equipoUsuarios = ficha.Equipo?.EquiposUsuarios
                                ?? new List<Equipos_Usuarios>();

            // 3) Filtrar sólo los Auditores y proyectar a Usuario
            var auditores = equipoUsuarios
                .Where(eu =>
                    eu.RolEnEquipo?
                      .Equals("Auditor", StringComparison.OrdinalIgnoreCase) == true
                )
                .Select(eu => eu.Usuario)
                .ToList();

            ViewBag.Responsables = new SelectList(
                auditores,      // lista de Usuario
                "Id",           // valor del <option>
                "NombreUsuario",// texto del <option>
                paso2.Responsable  // seleccionadoF
            );

            // 4) Categorías 5M
            var categorias5M = await _context.Categorias5M.ToListAsync();
            ViewBag.Categorias5M = new SelectList(
                categorias5M, "IdCategoria5M", "Descripcion");
        }

        // POST: FichasTecnicas/{idFicha}/Paso2/EliminarCausa
        [HttpPost("EliminarCausa"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCausa(int idFicha, int idCausa)
        {
            var causa = await _context.CausasPasos
                .FirstOrDefaultAsync(c => c.IdFichaTecnica == idFicha && c.IdCausa == idCausa);

            if (causa != null)
            {
                _context.CausasPasos.Remove(causa);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { idFicha });
        }

        [HttpPost("EliminarCausaJson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCausaJson(int idFicha, int idCausa)
        {
            var causa = await _context.CausasPasos
                .FirstOrDefaultAsync(c => c.IdFichaTecnica == idFicha && c.IdCausa == idCausa);

            if (causa == null)
                return Json(new { success = false, message = "Causa no encontrada" });

            _context.CausasPasos.Remove(causa);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
