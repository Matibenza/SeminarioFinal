using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;            // ← Para ClaimTypes.NameIdentifier
using TesisPractica.Models;
using Microsoft.EntityFrameworkCore;

public class FichasTecnicasController : Controller
{
    public enum EstadoEnum
    {
        Pendiente = 1,
        EnProgreso = 2,
        Completado = 3,
        // …otros estados si los tenés…
    }
    private readonly TesisDbContext _context;

    public FichasTecnicasController(TesisDbContext context)
    {
        _context = context;
    }

    [Route("FichasTecnicas/Detalle/{id}")]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var ficha = await _context.FichasTecnicas
            .Include(ft => ft.NoConformidad)
                .ThenInclude(nc => nc.Pieza)
                    .ThenInclude(p => p.Cliente)
            .Include(ft => ft.NoConformidad.Proceso)
            .Include(ft => ft.NoConformidad.DefectosNC)
                .ThenInclude(dnc => dnc.Defecto)
            .Include(ft => ft.Equipo)
                .ThenInclude(eq => eq.EquiposUsuarios)
                    .ThenInclude(eu => eu.Usuario)
                                 .Include(ft => ft.Paso3)
                                  .Include(ft => ft.Paso4)
                                 .Include(ft => ft.Paso5)
                                 .Include(ft => ft.Paso6)
            .FirstOrDefaultAsync(ft => ft.IdFichaTecnica == id);
       
        if (ficha == null)
            return NotFound();

        // ——————————————————————————————————————————
        // Defectos
        var defectos = ficha.NoConformidad?.DefectosNC
            .Select(dnc => dnc.Defecto?.NombreDefecto)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList() ?? new List<string>();
        ViewBag.Defectos = defectos;

        // Cargar Paso 0 Contención
        var paso0 = await _context.Paso0Contenciones
            .FirstOrDefaultAsync(p => p.IdFichaTecnica == id);

        // Pasar datos a ViewBag para la vista
        ViewBag.AccionContencion = paso0?.AccionContencion ?? "-";
        ViewBag.FechaInicioPaso0 = paso0 != null && paso0.FechaInicio != default(DateTime)
            ? paso0.FechaInicio.ToString("dd/MM/yyyy")
            : "-";
        ViewBag.ResponsablePaso0 = null;

        if (paso0 != null)
        {
            var responsableUsuario = await _context.Usuarios.FindAsync(paso0.Responsable);
            ViewBag.ResponsablePaso0 = responsableUsuario?.NombreUsuario ?? "-";
        }
        else
        {
            ViewBag.ResponsablePaso0 = "-";
        }

        // Paso 1: Verificación
        var paso1 = await _context.Paso1Verificaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdFichaTecnica == id);

        ViewBag.QueSucedePaso1 = paso1?.QueSucede ?? "-";
        ViewBag.FechaInicioPaso1 = paso1?.FechaInicio.ToString("dd/MM/yyyy") ?? "-";

        if (paso1 != null)
        {
            var responsablePaso1 = await _context.Usuarios.FindAsync(paso1.Responsable);
            ViewBag.ResponsablePaso1 = responsablePaso1?.NombreUsuario ?? "-";
        }
        else
        {
            ViewBag.ResponsablePaso1 = "-";
        }

        // Paso 2: Análisis de causa
        var paso2 = await _context.Paso2AnalisisCausas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdFichaTecnica == id);

        ViewBag.FechaInicioPaso2 = paso2?.FechaInicio.ToString("dd/MM/yyyy") ?? "-";

        if (paso2 != null)
        {
            var responsablePaso2 = await _context.Usuarios.FindAsync(paso2.Responsable);
            ViewBag.ResponsablePaso2 = responsablePaso2?.NombreUsuario ?? "-";
        }
        else
        {
            ViewBag.ResponsablePaso2 = "-";
        }

        // Datos del Paso 3 (si existe)
        if (ficha.Paso3 != null)
        {
            var respPaso3 = await _context.Usuarios
                .Where(u => u.Id == ficha.Paso3.Responsable)
                .Select(u => u.NombreUsuario)
                .FirstOrDefaultAsync();
            ViewBag.FechaInicioPaso3 = ficha.Paso3.FechaInicio.ToString("dd/MM/yyyy");
            ViewBag.ResponsablePaso3 = respPaso3 ?? "-";

            // Si querés que el detalle sea el último "porque" ingresado:
            var detalle = await _context.Analisis5Porques
                .Where(a => a.IdFichaTecnica == ficha.IdFichaTecnica)
                .OrderByDescending(a => a.IdAnalisis)
                .Select(a => a.QuintoPorque)
                .FirstOrDefaultAsync();

            ViewBag.DetallePaso3 = string.IsNullOrWhiteSpace(detalle) ? "—" : detalle;
        }
        else
        {
            ViewBag.FechaInicioPaso3 = "-";
            ViewBag.ResponsablePaso3 = "-";
            ViewBag.DetallePaso3 = "—";
        }

        // — Paso 4: Plan de Acción —
        var paso4 = ficha.Paso4;
        ViewBag.FechaInicioPaso4 = paso4?.FechaInicio.ToString("dd/MM/yyyy") ?? "-";

        if (paso4 != null)
        {
            var usuario4 = await _context.Usuarios.FindAsync(paso4.Responsable);
            ViewBag.ResponsablePaso4 = usuario4?.NombreUsuario ?? "-";
        }
        else
        {
            ViewBag.ResponsablePaso4 = "-";
        }

        // Conteo de tareas Pendiente + En Progreso
        var pendientes = await _context.Tareas
           .CountAsync(t =>
               t.IdFichaTecnica == id &&
               t.IdEstado == (int)EstadoEnum.Pendiente
           );
        var enProgreso = await _context.Tareas
            .CountAsync(t =>
                t.IdFichaTecnica == id &&
                t.IdEstado == (int)EstadoEnum.EnProgreso
            );
        ViewBag.TareasPorRealizarPaso4 = pendientes + enProgreso;


        // Paso 5: Diseño
        if (ficha.Paso5 != null)
        {
            ViewBag.FechaInicioPaso5 = ficha.Paso5.FechaInicio.ToString("dd/MM/yyyy");
            // buscamos el usuario responsable
            var resp5 = await _context.Usuarios
                           .Where(u => u.Id == ficha.Paso5.Responsable)
                           .Select(u => u.NombreUsuario)
                           .FirstOrDefaultAsync();
            ViewBag.ResponsablePaso5 = resp5 ?? "-";
        }
        else
        {
            ViewBag.FechaInicioPaso5 = "-";
            ViewBag.ResponsablePaso5 = "-";
        }

        // ── ViewBags para la tarjetita del Paso 6 ──
        ViewBag.FechaInicioPaso6 = ficha.Paso6?.FechaInicio.ToString("dd/MM/yyyy");

        // Responsable (busco el nombre en el equipo para evitar otra consulta)
        string responsablePaso6 = null;
        if (ficha.Paso6?.Responsable != null)
        {
            responsablePaso6 = ficha.Equipo?.EquiposUsuarios
                ?.FirstOrDefault(eu => eu.Usuario.Id == ficha.Paso6.Responsable)
                ?.Usuario?.NombreUsuario;
        }
        ViewBag.ResponsablePaso6 = responsablePaso6 ?? "-";

        // Estado (texto)
        string estado6 = null;
        if ((ficha.Paso6?.IdEstado ?? 0) > 0)
        {
            estado6 = await _context.Estados
                .Where(e => e.IdEstado == ficha.Paso6.IdEstado)
                .Select(e => e.Descripcion)
                .FirstOrDefaultAsync();
        }
        ViewBag.EstadoPaso6 = estado6 ?? "-";


        // Pieza, Cliente, Proceso
        ViewBag.Pieza = ficha.NoConformidad?.Pieza?.Descripcion ?? "—";
        ViewBag.Cliente = ficha.NoConformidad?.Pieza?.Cliente?.Nombre ?? "—";
        ViewBag.Proceso = ficha.NoConformidad?.Proceso?.Nombre ?? "—";

        // Equipo (Piloto + Miembros)
        if (ficha.Equipo != null)
        {
            var piloto = ficha.Equipo.EquiposUsuarios
                .FirstOrDefault(eu => eu.RolEnEquipo.ToLower() == "piloto")
                ?.Usuario?.NombreUsuario ?? "—";

            var miembros = ficha.Equipo.EquiposUsuarios
                .Where(eu => eu.RolEnEquipo.ToLower() != "piloto")
                .Select(eu => eu.Usuario?.NombreUsuario)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            ViewBag.Piloto = piloto;
            ViewBag.Miembros = miembros;
        }

        // 1) Obtengo el usuario completo (con su PK IdUsuario)
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);

        if (usuario == null)
            return Unauthorized();         // o NotFound, como prefieras

        ViewBag.RolUsuario = usuario.Rol;

        // 2) Uso usuario.IdUsuario (int) para filtrar equipos
        var equiposSupervisor = await _context.Equipos
            .Where(e => e.IdCreador == usuario.Id)
            .ToListAsync();
        ViewBag.EquiposSupervisor = equiposSupervisor;

        // ────────────────────────────────────────────────────────────
        ViewBag.TieneEquipo = ficha.Equipo != null && ficha.Equipo.EquiposUsuarios.Any();

        return View(ficha);
    }

    [HttpPost]
    public async Task<IActionResult> ActualizarFecha([FromBody] FechaUpdateDTO data)
    {
        var ficha = await _context.FichasTecnicas.FindAsync(data.IdFichaTecnica);
        if (ficha == null)
            return NotFound("Ficha no encontrada.");

        if (DateTime.TryParse(data.NuevaFecha, out DateTime fecha))
        {
            ficha.FechaFinEstimada = fecha;
            await _context.SaveChangesAsync();
            return Ok("Fecha actualizada");
        }

        return BadRequest("Formato de fecha inválido.");
    }

    public class FechaUpdateDTO
    {
        public int IdFichaTecnica { get; set; }
        public string NuevaFecha { get; set; }
    }

    // ─────────── NUEVO: acción para asignar equipo ───────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("FichasTecnicas/AsignarEquipo")]
    public async Task<IActionResult> AsignarEquipo(int idFicha, int idEquipo)
    {
        var ficha = await _context.FichasTecnicas.FindAsync(idFicha);
        if (ficha == null)
            return NotFound("Ficha no encontrada.");

        ficha.IdEquipo = idEquipo;
        await _context.SaveChangesAsync();

        var equipo = await _context.Equipos.FindAsync(idEquipo);
        return Json(new
        {
            success = true,
            nombre = equipo != null ? equipo.Nombre : ""
        });
    }

    // ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ObtenerDetalleNoConformidad(int id)
    {
        var nc = await _context.NoConformidades
            .Include(n => n.Pieza)
                .ThenInclude(p => p.Cliente)
            .Include(n => n.Proceso)
            .Include(n => n.Estado)
            .Include(n => n.Prioridad)
            .Include(n => n.Detectabilidad)
            .Include(n => n.DefectosNC)
                .ThenInclude(d => d.Defecto)
            .Include(n => n.CausasNC)
                .ThenInclude(c => c.Causa)
            .FirstOrDefaultAsync(n => n.IdNoConformidad == id);

        if (nc == null)
            return NotFound();

        var resultado = new
        {
            id = nc.IdNoConformidad,
            descripcionPieza = nc.Pieza?.Descripcion,
            descripcion = nc.Descripcion,
            cantidad = nc.Cantidad,
            consecuencia = nc.Consecuencia,
            estado = nc.Estado?.Descripcion,
            prioridad = nc.Prioridad?.Descripcion,
            proceso = nc.Proceso?.Nombre,
            detectabilidad = nc.Detectabilidad?.Descripcion,
            frecuencia = nc.Frecuencia,
            gravedad = nc.Gravedad,
            fechaCreacion = nc.FechaCreacion.ToString("dd/MM/yyyy"),
            fechaIncidente = nc.FechaIncidente == default
                                   ? null
                                   : nc.FechaIncidente.ToString("dd/MM/yyyy"),
            fechaProduccion = nc.FechaProduccion == default
                                   ? null
                                   : nc.FechaProduccion.ToString("dd/MM/yyyy"),
            fechaFinalizacion = nc.FechaFinalizacion?.ToString("dd/MM/yyyy"),
            defectos = nc.DefectosNC.Select(d => d.Defecto.NombreDefecto).ToList(),
            causas = nc.CausasNC.Select(c => c.Causa.Descripcion).ToList()
        };

        return Json(resultado);
    }

    

}
