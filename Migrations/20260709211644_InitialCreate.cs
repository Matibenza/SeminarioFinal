using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesisPractica.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias5M",
                columns: table => new
                {
                    IdCategoria5M = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias5M", x => x.IdCategoria5M);
                });

            migrationBuilder.CreateTable(
                name: "Causa",
                columns: table => new
                {
                    IdCausa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Causa", x => x.IdCausa);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IdCliente);
                });

            migrationBuilder.CreateTable(
                name: "Defectos",
                columns: table => new
                {
                    IdDefecto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreDefecto = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defectos", x => x.IdDefecto);
                });

            migrationBuilder.CreateTable(
                name: "Detectabilidad",
                columns: table => new
                {
                    IdDetectabilidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detectabilidad", x => x.IdDetectabilidad);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.IdEstado);
                });

            migrationBuilder.CreateTable(
                name: "Prioridad",
                columns: table => new
                {
                    IdPrioridad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prioridad", x => x.IdPrioridad);
                });

            migrationBuilder.CreateTable(
                name: "Procesos",
                columns: table => new
                {
                    IdProcesos = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procesos", x => x.IdProcesos);
                });

            migrationBuilder.CreateTable(
                name: "Tipos_Acciones",
                columns: table => new
                {
                    IdTipoAccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipos_Acciones", x => x.IdTipoAccion);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contrasena = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piezas",
                columns: table => new
                {
                    IdPieza = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piezas", x => x.IdPieza);
                    table.ForeignKey(
                        name: "FK_Piezas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    IdEquipo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdCreador = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.IdEquipo);
                    table.ForeignKey(
                        name: "FK_Equipos_Usuarios_IdCreador",
                        column: x => x.IdCreador,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResets_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoConformidad",
                columns: table => new
                {
                    IdNoConformidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaIncidente = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaProduccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Frecuencia = table.Column<int>(type: "int", nullable: true),
                    Recurrencia = table.Column<bool>(type: "bit", nullable: false),
                    Gravedad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdPieza = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: true),
                    Consecuencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdProceso = table.Column<int>(type: "int", nullable: false),
                    IdDetectabilidad = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    IdPrioridad = table.Column<int>(type: "int", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoConformidad", x => x.IdNoConformidad);
                    table.ForeignKey(
                        name: "FK_NoConformidad_Detectabilidad_IdDetectabilidad",
                        column: x => x.IdDetectabilidad,
                        principalTable: "Detectabilidad",
                        principalColumn: "IdDetectabilidad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoConformidad_Estados_IdEstado",
                        column: x => x.IdEstado,
                        principalTable: "Estados",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoConformidad_Piezas_IdPieza",
                        column: x => x.IdPieza,
                        principalTable: "Piezas",
                        principalColumn: "IdPieza",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoConformidad_Prioridad_IdPrioridad",
                        column: x => x.IdPrioridad,
                        principalTable: "Prioridad",
                        principalColumn: "IdPrioridad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoConformidad_Procesos_IdProceso",
                        column: x => x.IdProceso,
                        principalTable: "Procesos",
                        principalColumn: "IdProcesos",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoConformidad_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipos_Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdEquipo = table.Column<int>(type: "int", nullable: false),
                    RolEnEquipo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos_Usuarios", x => new { x.IdUsuario, x.IdEquipo });
                    table.ForeignKey(
                        name: "FK_Equipos_Usuarios_Equipos_IdEquipo",
                        column: x => x.IdEquipo,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Equipos_Usuarios_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Causa_NC",
                columns: table => new
                {
                    IdCausa = table.Column<int>(type: "int", nullable: false),
                    IdNoConformidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Causa_NC", x => new { x.IdCausa, x.IdNoConformidad });
                    table.ForeignKey(
                        name: "FK_Causa_NC_Causa_IdCausa",
                        column: x => x.IdCausa,
                        principalTable: "Causa",
                        principalColumn: "IdCausa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Causa_NC_NoConformidad_IdNoConformidad",
                        column: x => x.IdNoConformidad,
                        principalTable: "NoConformidad",
                        principalColumn: "IdNoConformidad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Defectos_NC",
                columns: table => new
                {
                    IdDefecto = table.Column<int>(type: "int", nullable: false),
                    IdNoConformidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defectos_NC", x => new { x.IdDefecto, x.IdNoConformidad });
                    table.ForeignKey(
                        name: "FK_Defectos_NC_Defectos_IdDefecto",
                        column: x => x.IdDefecto,
                        principalTable: "Defectos",
                        principalColumn: "IdDefecto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Defectos_NC_NoConformidad_IdNoConformidad",
                        column: x => x.IdNoConformidad,
                        principalTable: "NoConformidad",
                        principalColumn: "IdNoConformidad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichaTecnica",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNoConformidad = table.Column<int>(type: "int", nullable: false),
                    IdEquipo = table.Column<int>(type: "int", nullable: true),
                    IdEstado = table.Column<int>(type: "int", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichaTecnica", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_FichaTecnica_Equipos_IdEquipo",
                        column: x => x.IdEquipo,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FichaTecnica_Estados_IdEstado",
                        column: x => x.IdEstado,
                        principalTable: "Estados",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FichaTecnica_NoConformidad_IdNoConformidad",
                        column: x => x.IdNoConformidad,
                        principalTable: "NoConformidad",
                        principalColumn: "IdNoConformidad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorridaProduccion",
                columns: table => new
                {
                    IdProduccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    CantidadProducida = table.Column<int>(type: "int", nullable: false),
                    CantidadOK = table.Column<int>(type: "int", nullable: false),
                    CantidadNoOK = table.Column<int>(type: "int", nullable: false),
                    FechaProduccion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorridaProduccion", x => x.IdProduccion);
                    table.ForeignKey(
                        name: "FK_CorridaProduccion_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso0_Contencion",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccionContencion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MetodoControl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deposito_CantControlada = table.Column<int>(type: "int", nullable: true),
                    Deposito_CantSospechosa = table.Column<int>(type: "int", nullable: true),
                    Deposito_CantOk = table.Column<int>(type: "int", nullable: true),
                    Almacen_CantControlada = table.Column<int>(type: "int", nullable: true),
                    Almacen_CantSospechosa = table.Column<int>(type: "int", nullable: true),
                    Almacen_CantOk = table.Column<int>(type: "int", nullable: true),
                    BordeLinea_CantControlada = table.Column<int>(type: "int", nullable: true),
                    BordeLinea_CantSospechosa = table.Column<int>(type: "int", nullable: true),
                    BordeLinea_CantOk = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso0_Contencion", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso0_Contencion_Estados_IdEstado",
                        column: x => x.IdEstado,
                        principalTable: "Estados",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Paso0_Contencion_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Paso0_Contencion_Usuarios_Responsable",
                        column: x => x.Responsable,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso1",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    TipoProblema = table.Column<short>(type: "smallint", nullable: true),
                    Turno = table.Column<short>(type: "smallint", nullable: true),
                    Objetivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QueSucede = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QuienDetecta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CuandoSucede = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComoSucede = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CualPieza = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdOperador = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso1", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso1_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso2",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso2", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso2_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso3",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso3", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso3_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso4",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso4", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso4_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso5",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    FacilidadSeguimiento = table.Column<short>(type: "smallint", nullable: true),
                    Instrucciones = table.Column<short>(type: "smallint", nullable: true),
                    EstandarCalidad = table.Column<short>(type: "smallint", nullable: true),
                    Defectos = table.Column<short>(type: "smallint", nullable: true),
                    ParametrosCeroDefectos = table.Column<short>(type: "smallint", nullable: true),
                    Variacion = table.Column<short>(type: "smallint", nullable: true),
                    VariacionTrabajo = table.Column<short>(type: "smallint", nullable: true),
                    LiberacionDefectos = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso5", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso5_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paso6",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paso6", x => x.IdFichaTecnica);
                    table.ForeignKey(
                        name: "FK_Paso6_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CausasPasos",
                columns: table => new
                {
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    IdCausa = table.Column<int>(type: "int", nullable: false),
                    IdCategoria5M = table.Column<int>(type: "int", nullable: false),
                    DescripcionCausa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResultadoVerificacion = table.Column<short>(type: "smallint", nullable: true),
                    ClasificacionImpacto = table.Column<short>(type: "smallint", nullable: true),
                    EsCausaRaiz = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CausasPasos", x => new { x.IdFichaTecnica, x.IdCausa });
                    table.ForeignKey(
                        name: "FK_CausasPasos_Categorias5M_IdCategoria5M",
                        column: x => x.IdCategoria5M,
                        principalTable: "Categorias5M",
                        principalColumn: "IdCategoria5M",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CausasPasos_Paso2_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "Paso2",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analisis5Porque",
                columns: table => new
                {
                    IdAnalisis = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    IdCausa = table.Column<int>(type: "int", nullable: false),
                    PrimerPorque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SegundoPorque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TercerPorque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CuartoPorque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuintoPorque = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analisis5Porque", x => x.IdAnalisis);
                    table.ForeignKey(
                        name: "FK_Analisis5Porque_CausasPasos_IdFichaTecnica_IdCausa",
                        columns: x => new { x.IdFichaTecnica, x.IdCausa },
                        principalTable: "CausasPasos",
                        principalColumns: new[] { "IdFichaTecnica", "IdCausa" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    IdTarea = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFichaTecnica = table.Column<int>(type: "int", nullable: false),
                    IdTipoAccion = table.Column<int>(type: "int", nullable: false),
                    FechaObjetivo = table.Column<DateTime>(type: "date", nullable: false),
                    FechaFinal = table.Column<DateTime>(type: "date", nullable: false),
                    IdPrioridad = table.Column<int>(type: "int", nullable: false),
                    IdResponsable = table.Column<int>(type: "int", nullable: false),
                    IdEstado = table.Column<int>(type: "int", nullable: false),
                    IdCausaPasos = table.Column<int>(type: "int", nullable: false),
                    AccionDeMejora = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescripcionDeMejora = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LugarAplicacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanalImplementacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.IdTarea);
                    table.ForeignKey(
                        name: "FK_Tareas_CausasPasos_IdFichaTecnica_IdCausaPasos",
                        columns: x => new { x.IdFichaTecnica, x.IdCausaPasos },
                        principalTable: "CausasPasos",
                        principalColumns: new[] { "IdFichaTecnica", "IdCausa" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tareas_Estados_IdEstado",
                        column: x => x.IdEstado,
                        principalTable: "Estados",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tareas_FichaTecnica_IdFichaTecnica",
                        column: x => x.IdFichaTecnica,
                        principalTable: "FichaTecnica",
                        principalColumn: "IdFichaTecnica",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tareas_Prioridad_IdPrioridad",
                        column: x => x.IdPrioridad,
                        principalTable: "Prioridad",
                        principalColumn: "IdPrioridad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tareas_Tipos_Acciones_IdTipoAccion",
                        column: x => x.IdTipoAccion,
                        principalTable: "Tipos_Acciones",
                        principalColumn: "IdTipoAccion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tareas_Usuarios_IdResponsable",
                        column: x => x.IdResponsable,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    IdComentario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IdTarea = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.IdComentario);
                    table.ForeignKey(
                        name: "FK_Comentarios_Tareas_IdTarea",
                        column: x => x.IdTarea,
                        principalTable: "Tareas",
                        principalColumn: "IdTarea",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comentarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificacionAcciones",
                columns: table => new
                {
                    IdVerificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTarea = table.Column<int>(type: "int", nullable: false),
                    EsEfectiva = table.Column<bool>(type: "bit", nullable: false),
                    MetodoConfirmacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FechaVerificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacionAcciones", x => x.IdVerificacion);
                    table.ForeignKey(
                        name: "FK_VerificacionAcciones_Tareas_IdTarea",
                        column: x => x.IdTarea,
                        principalTable: "Tareas",
                        principalColumn: "IdTarea",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analisis5Porque_IdFichaTecnica_IdCausa",
                table: "Analisis5Porque",
                columns: new[] { "IdFichaTecnica", "IdCausa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Causa_NC_IdNoConformidad",
                table: "Causa_NC",
                column: "IdNoConformidad");

            migrationBuilder.CreateIndex(
                name: "IX_CausasPasos_IdCategoria5M",
                table: "CausasPasos",
                column: "IdCategoria5M");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_IdTarea",
                table: "Comentarios",
                column: "IdTarea");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CorridaProduccion_IdFichaTecnica",
                table: "CorridaProduccion",
                column: "IdFichaTecnica",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Defectos_NC_IdNoConformidad",
                table: "Defectos_NC",
                column: "IdNoConformidad");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_IdCreador",
                table: "Equipos",
                column: "IdCreador");

            migrationBuilder.CreateIndex(
                name: "IX_Equipos_Usuarios_IdEquipo",
                table: "Equipos_Usuarios",
                column: "IdEquipo");

            migrationBuilder.CreateIndex(
                name: "IX_FichaTecnica_IdEquipo",
                table: "FichaTecnica",
                column: "IdEquipo");

            migrationBuilder.CreateIndex(
                name: "IX_FichaTecnica_IdEstado",
                table: "FichaTecnica",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_FichaTecnica_IdNoConformidad",
                table: "FichaTecnica",
                column: "IdNoConformidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NoConformidad_IdDetectabilidad",
                table: "NoConformidad",
                column: "IdDetectabilidad");

            migrationBuilder.CreateIndex(
                name: "IX_NoConformidad_IdEstado",
                table: "NoConformidad",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_NoConformidad_IdPieza",
                table: "NoConformidad",
                column: "IdPieza");

            migrationBuilder.CreateIndex(
                name: "IX_NoConformidad_IdPrioridad",
                table: "NoConformidad",
                column: "IdPrioridad");

            migrationBuilder.CreateIndex(
                name: "IX_NoConformidad_IdProceso",
                table: "NoConformidad",
                column: "IdProceso");

            migrationBuilder.CreateIndex(
                name: "IX_NoConformidad_IdUsuario",
                table: "NoConformidad",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Paso0_Contencion_IdEstado",
                table: "Paso0_Contencion",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Paso0_Contencion_Responsable",
                table: "Paso0_Contencion",
                column: "Responsable");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Piezas_IdCliente",
                table: "Piezas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_IdEstado",
                table: "Tareas",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_IdFichaTecnica_IdCausaPasos",
                table: "Tareas",
                columns: new[] { "IdFichaTecnica", "IdCausaPasos" });

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_IdPrioridad",
                table: "Tareas",
                column: "IdPrioridad");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_IdResponsable",
                table: "Tareas",
                column: "IdResponsable");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_IdTipoAccion",
                table: "Tareas",
                column: "IdTipoAccion");

            migrationBuilder.CreateIndex(
                name: "IX_VerificacionAcciones_IdTarea",
                table: "VerificacionAcciones",
                column: "IdTarea",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analisis5Porque");

            migrationBuilder.DropTable(
                name: "Causa_NC");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "CorridaProduccion");

            migrationBuilder.DropTable(
                name: "Defectos_NC");

            migrationBuilder.DropTable(
                name: "Equipos_Usuarios");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "Paso0_Contencion");

            migrationBuilder.DropTable(
                name: "Paso1");

            migrationBuilder.DropTable(
                name: "Paso3");

            migrationBuilder.DropTable(
                name: "Paso4");

            migrationBuilder.DropTable(
                name: "Paso5");

            migrationBuilder.DropTable(
                name: "Paso6");

            migrationBuilder.DropTable(
                name: "PasswordResets");

            migrationBuilder.DropTable(
                name: "VerificacionAcciones");

            migrationBuilder.DropTable(
                name: "Causa");

            migrationBuilder.DropTable(
                name: "Defectos");

            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "CausasPasos");

            migrationBuilder.DropTable(
                name: "Tipos_Acciones");

            migrationBuilder.DropTable(
                name: "Categorias5M");

            migrationBuilder.DropTable(
                name: "Paso2");

            migrationBuilder.DropTable(
                name: "FichaTecnica");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropTable(
                name: "NoConformidad");

            migrationBuilder.DropTable(
                name: "Detectabilidad");

            migrationBuilder.DropTable(
                name: "Estados");

            migrationBuilder.DropTable(
                name: "Piezas");

            migrationBuilder.DropTable(
                name: "Prioridad");

            migrationBuilder.DropTable(
                name: "Procesos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
