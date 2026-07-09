using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;

namespace TesisPractica.Controllers
{
    [Route("Tablero")]
    public class TableroController : Controller
    {
        private readonly TesisDbContext _context;
        public TableroController(TesisDbContext context) => _context = context;

        // GET: /Tablero
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            // 1) Usuario actual
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null)
                return Unauthorized();

            // 2) Traer únicamente las tareas donde el usuario es responsable
            var tareas = await _context.Tareas
                .Include(t => t.Estado)
                .Include(t => t.Prioridad)
                .Where(t => t.IdResponsable == usuario.Id)
                .OrderBy(t => t.FechaObjetivo)
                .ToListAsync();

            // 3) Separar por estado
            ViewBag.PorHacer = tareas
                .Where(t => t.Estado.Descripcion == "PENDIENTE")
                .ToList();
            ViewBag.EnProgreso = tareas
                .Where(t => t.Estado.Descripcion == "EN PROGRESO")
                .ToList();
            ViewBag.Finalizado = tareas
                .Where(t => t.Estado.Descripcion == "COMPLETADO")
                .ToList();

            // 4) Dropdown de estados (para el modal), sin “Terminado”
            var estadosAll = await _context.Estados
                .AsNoTracking()
                .ToListAsync();
            ViewBag.EstadosModal = new SelectList(
                estadosAll.Where(e => e.Descripcion != "Terminado"),
                "IdEstado", "Descripcion"
            );

            // 5) Dropdown de prioridades para el modal
            var prioridades = await _context.Prioridades
                .AsNoTracking()
                .ToListAsync();
            ViewBag.PrioridadesTarea = prioridades
                .Select(p => new SelectListItem
                {
                    Value = p.IdPrioridad.ToString(),
                    Text = p.Descripcion
                })
                .ToList();

            // 6) Dropdown de tipos de acción para el modal
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

            // 7) Empleados para el modal: solo él mismo
            ViewBag.EmpleadosTarea = new List<SelectListItem>
    {
        new SelectListItem
        {
            Value = usuario.Id.ToString(),
            Text  = usuario.NombreUsuario
        }
    };

            // 8) Inicializar causas y contador de comentarios
            ViewBag.CausasFenomeno = new List<SelectListItem>();
            ViewBag.ComentariosCount = 0;

            // 9) Dropdown de Fichas Técnicas
            // — buscamos el Id del estado “Terminado”
            var estadoTerminado = await _context.Estados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Descripcion == "Terminado");
            var stateId = estadoTerminado?.IdEstado;

            // — luego filtramos las fichas
            var fichas = await _context.FichasTecnicas
                .Include(f => f.NoConformidad)
                    .ThenInclude(nc => nc.Pieza)
                        .ThenInclude(p => p.Cliente)
                .Include(f => f.Paso3)
                .Include(f => f.Equipo)
                    .ThenInclude(eq => eq.EquiposUsuarios)
                .Where(f =>
                    f.Paso3 != null
                    && f.Paso3.IdEstado == stateId
                    && f.Equipo.EquiposUsuarios.Any(eu => eu.IdUsuario == usuario.Id)
                )
                .ToListAsync();

            var fichasVm = fichas
           .Select(f => new {
               IdFicha = f.IdFichaTecnica,
               Texto = $"FCA-{f.IdFichaTecnica:000} – {f.NoConformidad.Pieza.Cliente.Nombre}",
               Cliente = f.NoConformidad.Pieza.Cliente.Nombre,
               IdPieza = f.NoConformidad.Pieza.IdPieza    // aquí va el Id de la pieza
           })
           .ToList();

            ViewBag.FichasTecnicas = fichasVm;

            // 10) Finalmente, renderizamos la vista:
            return View();
        }

        [HttpGet("Get5PorquesPorFicha")]
        public async Task<IActionResult> Get5PorquesPorFicha(int idFicha)
        {
            var lista = await _context.Analisis5Porques
                .Where(a => a.IdFichaTecnica == idFicha)
                .Select(a => new {
                    value = a.IdCausa.ToString(),      // ahora sí mandas el Id de la causa
                    text = a.QuintoPorque
                })
                .Distinct()                         // para que no te duplique
                .ToListAsync();
            return Json(lista);
        }




        [HttpPost("CrearTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTarea(
         [Bind(
      "IdFichaTecnica,IdCausaPasos,AccionDeMejora,DescripcionDeMejora," +
      "CanalImplementacion,LugarAplicacion,FechaObjetivo,FechaFinal," +
      "IdResponsable,IdEstado,IdPrioridad,IdTipoAccion")]
    Tarea tarea)
        {
            if (!ModelState.IsValid)
            {
                // Si quieres, aquí podrías recargar los ViewBag y devolver View("Index", …)
                // Pero si prefieres ignorar validación y simplemente volver al tablero:
                return RedirectToAction("Index");
            }

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();

            // Al devolver RedirectToAction, el navegador recibe un 302 y carga /Tablero/Index
            return RedirectToAction("Index");
        }




        [HttpPost("ActualizarEstadoTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoTarea(int idTarea, string nuevoEstado)
        {
            // 1) Localizar la tarea
            var tarea = await _context.Tareas.FindAsync(idTarea);
            if (tarea == null)
                return NotFound($"Tarea {idTarea} no encontrada.");

            // 2) Normalizar el string de búsqueda
            var estadoBuscado = nuevoEstado?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(estadoBuscado))
                return BadRequest("Estado inválido.");

            // 3) Buscar el estado con comparación sencilla
            var estadoObj = await _context.Estados
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Descripcion.ToUpper() == estadoBuscado);
            if (estadoObj == null)
                return BadRequest($"Estado '{nuevoEstado}' desconocido.");

            // 4) Actualizar y guardar
            tarea.IdEstado = estadoObj.IdEstado;
            await _context.SaveChangesAsync();

            return Ok(new { idTarea = tarea.IdTarea, nuevoEstado = estadoObj.Descripcion });
        }


        [HttpGet("GetComentariosTarea")]
        public async Task<IActionResult> GetComentariosTarea(int idTarea)
        {
            var comentarios = await _context.Comentarios
                .Where(c => c.IdTarea == idTarea)
                .Include(c => c.Usuario)  // para que NombreUsuario venga cargado
                .Select(c => new {
                    autor = c.Usuario.NombreUsuario,
                    texto = c.Descripcion,
                    fecha = c.FechaCreacion.ToString("o")
                })
                .ToListAsync();

            // Ok() devuelve JSON automáticamente
            return Ok(comentarios);
        }

        // POST /Tablero/AgregarComentario
        [HttpPost("AgregarComentario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(ComentarioDto dto)
        {
            // 1) Validación básica
            if (dto == null || dto.IdTarea <= 0 || string.IsNullOrWhiteSpace(dto.Texto))
                return BadRequest("Datos inválidos.");

            // 2) Usuario actual
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null)
                return Unauthorized();

            // 3) Crear y guardar comentario
            var comment = new Comentario
            {
                IdTarea = dto.IdTarea,
                Descripcion = dto.Texto.Trim(),
                UsuarioId = usuario.Id,
                FechaCreacion = DateTime.Now
            };
            _context.Comentarios.Add(comment);
            await _context.SaveChangesAsync();

            // 4) Devolver JSON con los datos del comentario nuevo
            return Ok(new
            {
                autor = usuario.NombreUsuario,
                fecha = comment.FechaCreacion.ToString("yyyy-MM-dd HH:mm"),
                texto = comment.Descripcion
            });
        }

        // GET /Tablero/GetTarea?idTarea=123
        [HttpGet("GetTarea")]
        public async Task<IActionResult> GetTarea(int idTarea)
        {
            var t = await _context.Tareas
                .Where(x => x.IdTarea == idTarea)
                // No hace falta un Include explícito cuando proyectas con navigation properties:
                .Select(x => new {
                    idTarea = x.IdTarea,
                    idFichaTecnica = x.IdFichaTecnica,
                    idCausaPasos = x.IdCausaPasos,
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
                    // estos dos son los nuevos campos:
                    cliente = x.FichaTecnica
                                       .NoConformidad
                                       .Pieza
                                       .Cliente
                                       .Nombre,
                    idPieza = x.FichaTecnica
                                       .NoConformidad
                                       .Pieza
                                       .IdPieza
                })
                .FirstOrDefaultAsync();

            if (t == null)
                return NotFound($"Tarea {idTarea} no encontrada.");

            return Ok(t);
        }
        // POST /Tablero/EliminarTarea
        [HttpPost("EliminarTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarTarea(int idTarea)
        {
            // 1) Localizar la tarea
            var tarea = await _context.Tareas.FindAsync(idTarea);
            if (tarea == null)
                return NotFound($"Tarea {idTarea} no encontrada.");

            // 2) Eliminar comentarios asociados
            var comentarios = _context.Comentarios
                .Where(c => c.IdTarea == idTarea);
            _context.Comentarios.RemoveRange(comentarios);

            // 3) Eliminar la propia tarea
            _context.Tareas.Remove(tarea);

            // 4) Guardar cambios
            await _context.SaveChangesAsync();

            // 5) Devolver OK para el cliente
            return Ok();
        }

        // POST /Tablero/EditarTarea
        [HttpPost("EditarTarea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTarea(TareaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tarea = await _context.Tareas.FindAsync(dto.IdTarea);
            if (tarea == null)
                return NotFound($"Tarea {dto.IdTarea} no encontrada.");

            // Actualizo campos
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

            // Volvemos al Tablero
            return RedirectToAction("Index");
        }

    }
}
