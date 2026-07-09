    using Microsoft.EntityFrameworkCore;

    namespace TesisPractica.Models
    {
        public class TesisDbContext : DbContext
        {
            public TesisDbContext(DbContextOptions<TesisDbContext> options) : base(options) { }

            // Tablas principales
            public DbSet<Usuario> Usuarios { get; set; }
            public DbSet<PasswordReset> PasswordResets { get; set; }
            public DbSet<Notificacion> Notificaciones { get; set; }
            public DbSet<NoConformidad> NoConformidades { get; set; }
            public DbSet<Causa> Causas { get; set; }
            public DbSet<Defecto> Defectos { get; set; }
            public DbSet<Pieza> Piezas { get; set; }
            public DbSet<Detectabilidad> Detectabilidades { get; set; }
            public DbSet<Cliente> Clientes { get; set; }
            public DbSet<Proceso> Procesos { get; set; }
            public DbSet<Estado> Estados { get; set; }
            public DbSet<Prioridad> Prioridades { get; set; }
            public DbSet<Equipo> Equipos { get; set; }

            // Tablas puente
            public DbSet<Causa_NC> CausasNC { get; set; }
            public DbSet<Defecto_NC> DefectosNC { get; set; }
            public DbSet<Equipos_Usuarios> EquiposUsuarios { get; set; }

            // Ficha Técnica y pasos
            public DbSet<FichaTecnica> FichasTecnicas { get; set; }
            public DbSet<Paso0Contencion> Paso0Contenciones { get; set; }
            public DbSet<Paso1Verificacion> Paso1Verificaciones { get; set; }
            public DbSet<Paso2AnalisisCausa> Paso2AnalisisCausas { get; set; }
            public DbSet<Paso3> Paso3 { get; set; }
            public DbSet<Paso4> Paso4 { get; set; }
            public DbSet<Paso5> Paso5 { get; set; }
            public DbSet<Paso6> Paso6 { get; set; }
            public DbSet<Categoria5M> Categorias5M { get; set; }
            public DbSet<CausaPaso> CausasPasos { get; set; }
            public DbSet<Analisis5Porque> Analisis5Porques { get; set; }
            public DbSet<Tarea> Tareas { get; set; }
            public DbSet<VerificacionAccion> VerificacionesAcciones { get; set; }
            public DbSet<CorridaProduccion> CorridasProduccion { get; set; }


            public DbSet<TipoAccion> TiposAcciones { get; set; }
            public DbSet<Comentario> Comentarios { get; set; }


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Nombrar tablas explícitamente
                modelBuilder.Entity<Usuario>().ToTable("Usuarios");
                modelBuilder.Entity<PasswordReset>().ToTable("PasswordResets");
                modelBuilder.Entity<Notificacion>().ToTable("Notificaciones");
                modelBuilder.Entity<NoConformidad>().ToTable("NoConformidad");
                modelBuilder.Entity<Causa>().ToTable("Causa");
                modelBuilder.Entity<Defecto>().ToTable("Defectos");
                modelBuilder.Entity<Pieza>().ToTable("Piezas");
                modelBuilder.Entity<Cliente>().ToTable("Clientes");
                modelBuilder.Entity<Proceso>().ToTable("Procesos");
                modelBuilder.Entity<Estado>().ToTable("Estados");
                modelBuilder.Entity<Prioridad>().ToTable("Prioridad");
                modelBuilder.Entity<Causa_NC>().ToTable("Causa_NC");
                modelBuilder.Entity<Defecto_NC>().ToTable("Defectos_NC");
                modelBuilder.Entity<Equipos_Usuarios>().ToTable("Equipos_Usuarios");
                modelBuilder.Entity<Equipo>().ToTable("Equipos");
                modelBuilder.Entity<FichaTecnica>().ToTable("FichaTecnica");
                modelBuilder.Entity<Paso0Contencion>().ToTable("Paso0_Contencion");
                modelBuilder.Entity<Paso1Verificacion>().ToTable("Paso1");
                modelBuilder.Entity<Paso2AnalisisCausa>().ToTable("Paso2");
                modelBuilder.Entity<Paso3>().ToTable("Paso3");
                modelBuilder.Entity<Paso5>().ToTable("Paso5");
                modelBuilder.Entity<Paso6>().ToTable("Paso6");
                modelBuilder.Entity<TipoAccion>().ToTable("Tipos_Acciones");    // o el nombre que uses en la BD
                modelBuilder.Entity<Comentario>().ToTable("Comentarios");
                modelBuilder.Entity<Tarea>().ToTable("Tareas");
                modelBuilder.Entity<VerificacionAccion>().ToTable("VerificacionAcciones");
                modelBuilder.Entity<CorridaProduccion>().ToTable("CorridaProduccion");




                // PK compuestas
                modelBuilder.Entity<Causa_NC>().HasKey(cn => new { cn.IdCausa, cn.IdNoConformidad });
                modelBuilder.Entity<Defecto_NC>().HasKey(dn => new { dn.IdDefecto, dn.IdNoConformidad });
                modelBuilder.Entity<Equipos_Usuarios>().HasKey(eu => new { eu.IdUsuario, eu.IdEquipo });
                modelBuilder.Entity<Paso0Contencion>().HasKey(p => p.IdFichaTecnica);
                modelBuilder.Entity<Paso1Verificacion>().HasKey(p => p.IdFichaTecnica);
                modelBuilder.Entity<Paso2AnalisisCausa>().HasKey(p => p.IdFichaTecnica);
                modelBuilder.Entity<Paso3>().HasKey(p => p.IdFichaTecnica);
                modelBuilder.Entity<Paso4>().ToTable("Paso4").HasKey(p => p.IdFichaTecnica);
                modelBuilder.Entity<Paso5>().HasKey(p => p.IdFichaTecnica);
                modelBuilder.Entity<CausaPaso>().HasKey(cp => new { cp.IdFichaTecnica, cp.IdCausa });
                modelBuilder.Entity<Paso6>().HasKey(p => p.IdFichaTecnica);


                // Relaciones Equipos_Usuarios
                modelBuilder.Entity<Equipos_Usuarios>()
                    .HasOne(eu => eu.Usuario)
                    .WithMany(u => u.Equipos_Usuarios)
                    .HasForeignKey(eu => eu.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Equipos_Usuarios>()
                    .HasOne(eu => eu.Equipo)
                    .WithMany(e => e.EquiposUsuarios)
                    .HasForeignKey(eu => eu.IdEquipo)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación Equipo -> Creador
                modelBuilder.Entity<Equipo>()
                    .HasOne(e => e.Creador)
                    .WithMany()
                    .HasForeignKey(e => e.IdCreador)
                    .OnDelete(DeleteBehavior.Restrict);

                // PasswordReset -> Usuario
                modelBuilder.Entity<PasswordReset>()
                    .HasOne(p => p.Usuario)
                    .WithMany(u => u.PasswordResets)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Notificacion -> Usuario
                modelBuilder.Entity<Notificacion>()
                    .HasOne(n => n.Usuario)
                    .WithMany(u => u.Notificaciones)
                    .HasForeignKey(n => n.UsuarioId);

                // NoConformidad -> Usuario, Pieza, Proceso, Estado, Prioridad
                modelBuilder.Entity<NoConformidad>()
                    .HasOne(nc => nc.Usuario)
                    .WithMany()
                    .HasForeignKey(nc => nc.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<NoConformidad>()
                    .HasOne(nc => nc.Pieza)
                    .WithMany(p => p.NoConformidades)
                    .HasForeignKey(nc => nc.IdPieza)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<NoConformidad>()
                    .HasOne(nc => nc.Proceso)
                    .WithMany()
                    .HasForeignKey(nc => nc.IdProceso)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<NoConformidad>()
                    .HasOne(nc => nc.Estado)
                    .WithMany()
                    .HasForeignKey(nc => nc.IdEstado)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<NoConformidad>()
                    .HasOne(nc => nc.Prioridad)
                    .WithMany()
                    .HasForeignKey(nc => nc.IdPrioridad)
                    .OnDelete(DeleteBehavior.Restrict);

                // Pieza -> Cliente
                modelBuilder.Entity<Pieza>()
                    .HasOne(p => p.Cliente)
                    .WithMany(c => c.Piezas)
                    .HasForeignKey(p => p.IdCliente)
                    .OnDelete(DeleteBehavior.SetNull);

                // FichaTecnica -> NoConformidad (1:1)
                modelBuilder.Entity<FichaTecnica>()
                    .HasOne(ft => ft.NoConformidad)
                    .WithOne(nc => nc.FichaTecnica)
                    .HasForeignKey<FichaTecnica>(ft => ft.IdNoConformidad)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // FichaTecnica -> Equipo, Estado (1:N opcionales)
                modelBuilder.Entity<FichaTecnica>()
                    .HasOne(ft => ft.Equipo)
                    .WithMany(e => e.FichasTecnicas)
                    .HasForeignKey(ft => ft.IdEquipo)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<FichaTecnica>()
                    .HasOne(ft => ft.Estado)
                    .WithMany(s => s.FichasTecnicas)
                    .HasForeignKey(ft => ft.IdEstado)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                // Paso0Contencion 1:1
                modelBuilder.Entity<Paso0Contencion>()
                   .HasOne(p => p.FichaTecnica)
                   .WithOne(f => f.Paso0Contencion)
                   .HasForeignKey<Paso0Contencion>(p => p.IdFichaTecnica)
                   .OnDelete(DeleteBehavior.Cascade);

                // Paso1Verificacion 1:1
                modelBuilder.Entity<Paso1Verificacion>()
                   .HasOne(p => p.FichaTecnica)
                   .WithOne(ft => ft.Paso1Verificacion)
                   .HasForeignKey<Paso1Verificacion>(p => p.IdFichaTecnica);

                // Paso2AnalisisCausa 1:1
                modelBuilder.Entity<Paso2AnalisisCausa>()
                   .HasOne(p => p.FichaTecnica)
                   .WithOne(ft => ft.Paso2AnalisisCausa)
                   .HasForeignKey<Paso2AnalisisCausa>(p => p.IdFichaTecnica)
                   .OnDelete(DeleteBehavior.Cascade);

                // Paso3 1:1 con FichaTecnica
                modelBuilder.Entity<Paso3>()
                    .HasOne(p => p.FichaTecnica)
                    .WithOne(f => f.Paso3)
                    .HasForeignKey<Paso3>(p => p.IdFichaTecnica)
                    .OnDelete(DeleteBehavior.Cascade);

                // Paso4 1:1 con FichaTecnica
                modelBuilder.Entity<Paso4>()
                    .HasOne(p => p.FichaTecnica)
                    .WithOne(f => f.Paso4)
                    .HasForeignKey<Paso4>(p => p.IdFichaTecnica)
                    .OnDelete(DeleteBehavior.Cascade);

                // Paso5 1:1
                modelBuilder.Entity<Paso5>()
                    .HasOne(p => p.FichaTecnica)
                    .WithOne(ft => ft.Paso5)
                    .HasForeignKey<Paso5>(p => p.IdFichaTecnica)
                    .OnDelete(DeleteBehavior.Cascade);

                // Paso6 1:1 con FichaTecnica
                modelBuilder.Entity<Paso6>()
                    .HasOne(p => p.FichaTecnica)
                    .WithOne(ft => ft.Paso6)
                    .HasForeignKey<Paso6>(p => p.IdFichaTecnica)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación 1:1 entre CausaPaso (PK compuesta) y Analisis5Porque
                modelBuilder.Entity<Analisis5Porque>()
                    .HasOne(a => a.CausaPaso)
                    .WithOne(c => c.Analisis5Porque)
                    .HasForeignKey<Analisis5Porque>(a => new { a.IdFichaTecnica, a.IdCausa })
                    .OnDelete(DeleteBehavior.Cascade);


                // Categoria5M
                modelBuilder.Entity<Categoria5M>()
                    .ToTable("Categorias5M")
                    .HasKey(c => c.IdCategoria5M);

                modelBuilder.Entity<Categoria5M>()
                    .Property(c => c.Descripcion)
                    .HasMaxLength(100)
                    .IsRequired();

                // CausaPaso
                modelBuilder.Entity<CausaPaso>()
                 .ToTable("CausasPasos")
                 .HasKey(cp => new { cp.IdFichaTecnica, cp.IdCausa });

                modelBuilder.Entity<CausaPaso>()
                    .HasOne(cp => cp.Paso2)
                    .WithMany(p2 => p2.CausasPasos)
                    .HasForeignKey(cp => cp.IdFichaTecnica)
                    .OnDelete(DeleteBehavior.Cascade);


                modelBuilder.Entity<CausaPaso>()
                    .HasOne(cp => cp.Categoria5M)
                    .WithMany()
                    .HasForeignKey(cp => cp.IdCategoria5M)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Tarea>(entity =>
                {
                    entity.HasKey(e => e.IdTarea);
                    entity.Property(e => e.IdTarea)
                          .ValueGeneratedOnAdd();

                    entity.Property(e => e.FechaObjetivo)
                          .HasColumnType("date");
                    entity.Property(e => e.FechaFinal)
                          .HasColumnType("date");

                    entity
                      .HasOne(e => e.FichaTecnica)
                      .WithMany(f => f.Tareas)
                      .HasForeignKey(e => e.IdFichaTecnica)
                      .OnDelete(DeleteBehavior.Restrict);

                    entity
                      .HasOne(e => e.TipoAccion)
                      .WithMany(t => t.Tareas)
                      .HasForeignKey(e => e.IdTipoAccion)
                      .OnDelete(DeleteBehavior.Restrict);

                    entity
                      .HasOne(e => e.Prioridad)
                      .WithMany(p => p.Tareas)
                      .HasForeignKey(e => e.IdPrioridad)
                      .OnDelete(DeleteBehavior.Restrict);

                    entity
                      .HasOne(e => e.Responsable)
                      .WithMany(u => u.TareasAsignadas)
                      .HasForeignKey(e => e.IdResponsable)
                      .OnDelete(DeleteBehavior.Restrict);

                    entity
                      .HasOne(e => e.Estado)
                      .WithMany(s => s.Tareas)
                      .HasForeignKey(e => e.IdEstado)
                      .OnDelete(DeleteBehavior.Restrict);

                    entity
                      .HasOne(t => t.CausaPaso)
                      .WithMany(cp => cp.Tareas)
                      .HasForeignKey(t => new { t.IdFichaTecnica, t.IdCausaPasos })
                      .OnDelete(DeleteBehavior.Restrict);

                    entity
                  .HasMany(e => e.Comentarios)
                  .WithOne(c => c.Tarea)
                  .HasForeignKey(c => c.IdTarea)
                  .OnDelete(DeleteBehavior.Restrict);
                });

                //VerificacionAccion 1:1 con Tarea
                modelBuilder.Entity<Tarea>()
                    .HasOne(t => t.VerificacionAccion)
                    .WithOne(v => v.Tarea)
                    .HasForeignKey<VerificacionAccion>(v => v.IdTarea)
                    .OnDelete(DeleteBehavior.Cascade);

                // CorridaProduccion 1:1 con FichaTecnica
                modelBuilder.Entity<CorridaProduccion>()
                    .HasOne(cp => cp.FichaTecnica)
                    .WithOne(ft => ft.CorridaProduccion)
                    .HasForeignKey<CorridaProduccion>(cp => cp.IdFichaTecnica)
                    .OnDelete(DeleteBehavior.Cascade);

            }

        }
    }