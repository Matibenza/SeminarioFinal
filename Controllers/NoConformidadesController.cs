using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesisPractica.Models;
using TesisPractica.ViewModels;

namespace TesisPractica.Controllers
{
    public class NoConformidadesController : Controller
    {
        private readonly TesisDbContext _context;

        public NoConformidadesController(TesisDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? idCliente, DateTime? fecha)
        {

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null) return RedirectToAction("Index", "Login");

            ViewBag.RolUsuario = usuario.Rol;
            ViewBag.Clientes = await _context.Clientes.ToListAsync();
            var query = _context.NoConformidades
            .Include(nc => nc.Pieza) // Necesario para acceder a IdCliente
                .Where(nc => nc.IdUsuario == usuario.Id);

            if (idCliente.HasValue && idCliente.Value > 0)
            {
                query = query.Where(nc => nc.Pieza.IdCliente == idCliente.Value);
            }

            if (fecha.HasValue)
            {
                var fechaSinHora = fecha.Value.Date;
                query = query.Where(nc => nc.FechaCreacion.Date == fechaSinHora);
            }

            var noConformidades = await query.ToListAsync();

            IQueryable<FichaTecnica> fichasQuery = _context.FichasTecnicas
        .Include(ft => ft.NoConformidad)
        .Include(ft => ft.Equipo)
            .ThenInclude(e => e.EquiposUsuarios);

            if (usuario.Rol.ToLower() == "supervisor")
            {
                fichasQuery = fichasQuery
                    .Where(ft => ft.NoConformidad.IdUsuario == usuario.Id);
            }
            else
            {
                fichasQuery = fichasQuery
                    .Where(ft =>
                        ft.Equipo != null
                        && ft.Equipo.EquiposUsuarios
                            .Any(eu => eu.IdUsuario == usuario.Id)
                    );
            }

            ViewBag.FichasTecnicas = await fichasQuery.ToListAsync();

            return View(noConformidades);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            // Preparamos el VM con catálogos
            var viewModel = new NoConformidadCreateViewModel
            {
                ListaClientes = await _context.Clientes.ToListAsync(),
                ListaProcesos = await _context.Procesos.ToListAsync(),
                ListaDetectabilidades = await _context.Detectabilidades.ToListAsync(),
                ListaDefectos = await _context.Defectos.ToListAsync(),
                ListaPrioridades = await _context.Prioridades.ToListAsync(),
                ListaEstados = await _context.Estados.ToListAsync(),
                // Fechas por defecto
                FechaCreacion = DateTime.Today,
                FechaIncidente = DateTime.Today,
                FechaVencimiento = DateTime.Today.AddMonths(1),
                FechaProduccion = DateTime.Today
            };

            if (id.HasValue)
            {
                // Estamos en modo edición: recuperamos la NC
                var nc = await _context.NoConformidades.FindAsync(id.Value);
                if (nc != null)
                {
                    // Determinar cliente actual a partir de la pieza
                    var clienteId = await _context.Piezas
                        .Where(p => p.IdPieza == nc.IdPieza)
                        .Select(p => p.IdCliente)
                        .FirstOrDefaultAsync();

                    // Rellenar campos básicos
                    viewModel.IdNoConformidad = nc.IdNoConformidad;
                    viewModel.IdCliente = clienteId;
                    viewModel.IdPieza = nc.IdPieza;
                    viewModel.Cantidad = nc.Cantidad;
                    viewModel.IdProceso = nc.IdProceso;
                    viewModel.Recurrente = nc.Recurrencia;
                    viewModel.Frecuencia = nc.Frecuencia;
                    viewModel.IdDetectabilidad = nc.IdDetectabilidad;
                    viewModel.FechaIncidente = nc.FechaIncidente;
                    viewModel.FechaProduccion = nc.FechaProduccion;
                    viewModel.FechaCreacion = nc.FechaCreacion;
                    viewModel.FechaVencimiento = nc.FechaFinalizacion;
                    viewModel.IdPrioridad = nc.IdPrioridad;
                    viewModel.IdEstado = nc.IdEstado;
                    viewModel.Gravedad = nc.Gravedad;
                    viewModel.Consecuencia = nc.Consecuencia;
                    viewModel.Descripcion = nc.Descripcion;

                    // Cargar piezas del cliente
                    viewModel.ListaPiezas = await _context.Piezas
                        .Where(p => p.IdCliente == clienteId)
                        .ToListAsync();

                    // Cargar defectos seleccionados
                    viewModel.IdDefecto = await _context.DefectosNC
                        .Where(d => d.IdNoConformidad == nc.IdNoConformidad)
                        .Select(d => d.IdDefecto)
                        .ToListAsync();

                    // **Cargar causas seleccionadas** (misma lógica que defectos)
                    viewModel.IdCausa = await _context.CausasNC
                        .Where(c => c.IdNoConformidad == nc.IdNoConformidad)
                        .Select(c => c.IdCausa)
                        .ToListAsync();
                }
                else
                {
                    // Si no encontramos la NC, devolvemos 404
                    return NotFound();
                }
            }
            else
            {
                // Nuevo registro: no hay piezas hasta que el usuario elija cliente
                viewModel.ListaPiezas = new List<Pieza>();
            }

            // Siempre cargamos el catálogo de causas para el dropdown
            viewModel.ListaCausas = await _context.Causas.ToListAsync();

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> CreateAjax(NoConformidadCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos" });

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == User.Identity.Name);
            if (usuario == null)
                return Json(new { success = false, message = "Usuario no encontrado" });

            try
            {
                if (model.IdNoConformidad > 0)
                {
                    // --- ACTUALIZAR EXISTENTE ---
                    var nc = await _context.NoConformidades
                        .FindAsync(model.IdNoConformidad.Value);
                    if (nc == null)
                        return Json(new { success = false, message = "NoConformidad no encontrada" });

                    // Actualizar campos
                    nc.IdPieza = model.IdPieza ?? 0;
                    nc.Cantidad = model.Cantidad;
                    nc.IdProceso = model.IdProceso ?? 0;
                    nc.Recurrencia = model.Recurrente;
                    nc.Frecuencia = model.Frecuencia;
                    nc.IdDetectabilidad = model.IdDetectabilidad ?? 0;
                    nc.FechaIncidente = model.FechaIncidente;
                    nc.FechaProduccion = model.FechaProduccion;
                    nc.FechaCreacion = model.FechaCreacion;
                    nc.FechaFinalizacion = model.FechaVencimiento;
                    nc.IdPrioridad = model.IdPrioridad ?? 0;
                    nc.IdEstado = model.IdEstado ?? 0;
                    nc.Gravedad = model.Gravedad;
                    nc.Consecuencia = model.Consecuencia;
                    nc.Descripcion = model.Descripcion;

                    await _context.SaveChangesAsync();

                    // Defectos: eliminar antiguos y añadir nuevos
                    _context.DefectosNC.RemoveRange(
                        _context.DefectosNC.Where(d => d.IdNoConformidad == nc.IdNoConformidad)
                    );
                    if (model.IdDefecto?.Any() == true)
                    {
                        _context.DefectosNC.AddRange(
                            model.IdDefecto.Select(idDef => new Defecto_NC
                            {
                                IdNoConformidad = nc.IdNoConformidad,
                                IdDefecto = idDef
                            })
                        );
                    }

                    // Causas: eliminar antiguas y añadir nuevas
                    _context.CausasNC.RemoveRange(
                        _context.CausasNC.Where(c => c.IdNoConformidad == nc.IdNoConformidad)
                    );
                    if (model.IdCausa?.Any() == true)
                    {
                        _context.CausasNC.AddRange(
                            model.IdCausa.Select(idCau => new Causa_NC
                            {
                                IdNoConformidad = nc.IdNoConformidad,
                                IdCausa = idCau
                            })
                        );
                    }

                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "No conformidad actualizada correctamente" });
                }
                else
                {
                    // --- CREAR NUEVA ---
                    var noConformidad = new NoConformidad
                    {
                        IdUsuario = usuario.Id,
                        IdPieza = model.IdPieza ?? 0,
                        Cantidad = model.Cantidad,
                        IdProceso = model.IdProceso ?? 0,
                        Recurrencia = model.Recurrente,
                        Frecuencia = model.Frecuencia,
                        IdDetectabilidad = model.IdDetectabilidad ?? 0,
                        FechaIncidente = model.FechaIncidente,
                        FechaProduccion = model.FechaProduccion,
                        FechaCreacion = model.FechaCreacion,
                        FechaFinalizacion = model.FechaVencimiento,
                        IdPrioridad = model.IdPrioridad ?? 0,
                        IdEstado = model.IdEstado ?? 0,
                        Gravedad = model.Gravedad,
                        Consecuencia = model.Consecuencia,
                        Descripcion = model.Descripcion
                    };

                    _context.NoConformidades.Add(noConformidad);
                    await _context.SaveChangesAsync();

                    // 2) Creamos automáticamente la FichaTecnica vinculada
                    var ficha = new FichaTecnica
                    {
                        IdNoConformidad = noConformidad.IdNoConformidad,
                        FechaCreacion = noConformidad.FechaCreacion
                        // Deja el resto de campos (IdEquipo, IdEstado, Descripcion, FechaFinEstimada) nulos por ahora
                    };
                    _context.FichasTecnicas.Add(ficha);
                    await _context.SaveChangesAsync();



                    // Defectos seleccionados
                    if (model.IdDefecto?.Any() == true)
                    {
                        _context.DefectosNC.AddRange(
                            model.IdDefecto.Select(idDef => new Defecto_NC
                            {
                                IdNoConformidad = noConformidad.IdNoConformidad,
                                IdDefecto = idDef
                            })
                        );
                    }

                    // Causas seleccionadas
                    if (model.IdCausa?.Any() == true)
                    {
                        _context.CausasNC.AddRange(
                            model.IdCausa.Select(idCau => new Causa_NC
                            {
                                IdNoConformidad = noConformidad.IdNoConformidad,
                                IdCausa = idCau
                            })
                        );
                    }



                    await _context.SaveChangesAsync();

                    // retorna también el nuevo Id
                    return Json(new
                    {
                        success = true,
                        message = "No conformidad creada correctamente",
                        idNoConformidad = noConformidad.IdNoConformidad
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetPiezasByCliente(int idCliente)
        {
            var piezas = await _context.Piezas
                .Where(p => p.IdCliente == idCliente)
                .Select(p => new { idPieza = p.IdPieza, descripcion = p.Descripcion })
                .ToListAsync();

            return Json(piezas);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var nc = await _context.NoConformidades
                .Include(n => n.Pieza)
                .Include(n => n.Proceso)
                .Include(n => n.Estado)
                .Include(n => n.Prioridad)
                .Include(n => n.Detectabilidad)
                // Relación con causas
                .Include(n => n.CausasNC)
                    .ThenInclude(cnc => cnc.Causa)
                // Relación con defectos
                .Include(n => n.DefectosNC)
                    .ThenInclude(dnc => dnc.Defecto)
                .FirstOrDefaultAsync(x => x.IdNoConformidad == id);

            if (nc == null) return NotFound();

            return Json(new
            {
                id = nc.IdNoConformidad,
                descripcionPieza = nc.Pieza?.Descripcion,
                descripcion = nc.Descripcion,
                cantidad = nc.Cantidad,
                consecuencia = nc.Consecuencia,
                nombreProceso = nc.Proceso?.Nombre,
                descripcionEstado = nc.Estado?.Descripcion,
                descripcionPrioridad = nc.Prioridad?.Descripcion,
                descripcionDetectabilidad = nc.Detectabilidad?.Descripcion,
                frecuencia = nc.Frecuencia,
                gravedad = nc.Gravedad,
                recurrencia = nc.Recurrencia ? "Recurrente" : "No recurrente",
                fechaCreacion = nc.FechaCreacion.ToString("dd/MM/yyyy"),
                fechaIncidente = nc.FechaIncidente.ToString("dd/MM/yyyy"),
                fechaProduccion = nc.FechaProduccion.ToString("dd/MM/yyyy"),
                fechaFinalizacion = nc.FechaFinalizacion?.ToString("dd/MM/yyyy"),

                // **AÑADIDAS AQUÍ** las dos colecciones:
                causas = nc.CausasNC.Select(x => x.Causa.Descripcion).ToList(),
                defectos = nc.DefectosNC.Select(x => x.Defecto.NombreDefecto).ToList()
            });
        }




        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var nc = await _context.NoConformidades.FindAsync(id);
            if (nc == null) return NotFound();

            try
            {
                _context.NoConformidades.Remove(nc);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error al eliminar la no conformidad: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerSupervisores()
        {
            var supervisores = await _context.Usuarios
                .Where(u => u.Rol.ToLower() == "supervisor")
                .Select(u => new { u.Id, u.NombreUsuario })
                .ToListAsync();

            return Json(supervisores);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AsignarEquipoATecnica([FromBody] AsignarEquipoDto dto)
        {
            var ficha = await _context.FichasTecnicas
                .FirstOrDefaultAsync(f => f.IdNoConformidad == dto.IdNoConformidad);
            if (ficha == null) return NotFound("Ficha técnica no encontrada");

            ficha.IdEquipo = dto.IdEquipo;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Equipo asignado correctamente" });
        }

        public class AsignarEquipoDto
        {
            public int IdNoConformidad { get; set; }
            public int IdEquipo { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Reasignar(int idNoConformidad, int idUsuario)
        {
            // 0) Validación temprana de parámetro
            if (idUsuario <= 0)
                return BadRequest($"Parámetro idUsuario inválido: {idUsuario}");

            // 1) Compruebo que el supervisor exista
            var supervisor = await _context.Usuarios.FindAsync(idUsuario);
            if (supervisor == null)
                return BadRequest($"No existe el usuario con Id = {idUsuario}");

            // 2) Cargo la NC y su ficha técnica
            var nc = await _context.NoConformidades.FindAsync(idNoConformidad);
            var ficha = await _context.FichasTecnicas
                            .FirstOrDefaultAsync(ft => ft.IdNoConformidad == idNoConformidad);
            if (nc == null || ficha == null)
                return BadRequest("No existe la NC o la ficha técnica");

            // 3) Cargo el equipo actual (si lo tuviera)
            Equipo equipoActual = null;
            if (ficha.IdEquipo.HasValue)
            {
                equipoActual = await _context.Equipos
                    .Include(e => e.EquiposUsuarios)
                    .FirstOrDefaultAsync(e => e.IdEquipo == ficha.IdEquipo.Value);
            }

            Equipo equipoParaAsignar = null;

            if (equipoActual != null)
            {
                // ¿El equipo actual está compartido por otras fichas?
                bool tieneOtras = await _context.FichasTecnicas
                    .AnyAsync(ft =>
                        ft.IdEquipo == equipoActual.IdEquipo
                     && ft.IdFichaTecnica != ficha.IdFichaTecnica);

                if (tieneOtras)
                {
                    // 3a) Preparo la lista de miembros actuales
                    var miembros = equipoActual.EquiposUsuarios
                                      .Select(eu => new { eu.IdUsuario, eu.RolEnEquipo })
                                      .ToList();

                    // 3b) Cargo todos los equipos que el nuevo supervisor ya creó
                    var candidatos = await _context.Equipos
                        .Where(e => e.IdCreador == idUsuario)
                        .Include(e => e.EquiposUsuarios)
                        .ToListAsync();

                    // 3c) Busco en memoria un "gemelo" con los mismos usuarios y roles
                    var candidato = candidatos.FirstOrDefault(e =>
                        e.EquiposUsuarios.Count == miembros.Count
                     && miembros.All(m =>
                            e.EquiposUsuarios.Any(eu =>
                                eu.IdUsuario == m.IdUsuario &&
                                eu.RolEnEquipo == m.RolEnEquipo
                            )
                        )
                    );

                    if (candidato != null)
                    {
                        equipoParaAsignar = candidato;
                    }
                    else
                    {
                        // 3d) Si no existe, clono el equipo original
                        var clon = new Equipo
                        {
                            Nombre = equipoActual.Nombre,
                            Descripcion = equipoActual.Descripcion,
                            FechaCreacion = DateTime.Now,
                            IdCreador = idUsuario
                        };
                        _context.Equipos.Add(clon);
                        await _context.SaveChangesAsync();

                        // Clono también las relaciones con usuarios
                        foreach (var eu in equipoActual.EquiposUsuarios)
                        {
                            _context.EquiposUsuarios.Add(new Equipos_Usuarios
                            {
                                IdEquipo = clon.IdEquipo,
                                IdUsuario = eu.IdUsuario,
                                RolEnEquipo = eu.RolEnEquipo
                            });
                        }
                        await _context.SaveChangesAsync();

                        equipoParaAsignar = clon;
                    }
                }
                else
                {
                    // 4) Equipo exclusivo: simplemente transfiero el creador
                    equipoActual.IdCreador = idUsuario;
                    await _context.SaveChangesAsync();
                    equipoParaAsignar = equipoActual;
                }
            }

            // 5) Finalmente reasigno la NoConformidad y la ficha técnica
            nc.IdUsuario = idUsuario;
            if (equipoParaAsignar != null)
                ficha.IdEquipo = equipoParaAsignar.IdEquipo;

            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
