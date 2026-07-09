using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("FichaTecnica")]
    public class FichaTecnica
    {
        [Key]
        public int IdFichaTecnica { get; set; }

        // FK obligatorio a la NoConformidad
        public int IdNoConformidad { get; set; }
        [ForeignKey(nameof(IdNoConformidad))]
        public NoConformidad NoConformidad { get; set; }

        // Pasos 1:1 (opcionales hasta existir)
        public Paso0Contencion? Paso0Contencion { get; set; }
        public Paso1Verificacion? Paso1Verificacion { get; set; }
        public Paso2AnalisisCausa? Paso2AnalisisCausa { get; set; }
        public Paso3? Paso3 { get; set; }
        public Paso4? Paso4 { get; set; }
        public Paso5? Paso5 { get; set; }
        public Paso6? Paso6 { get; set; }

        // Colección de Tareas (inicializada para evitar null refs)
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();

        // Equipo (nullable)
        public int? IdEquipo { get; set; }
        [ForeignKey(nameof(IdEquipo))]
        public Equipo? Equipo { get; set; }

        // Estado (nullable)
        public int? IdEstado { get; set; }
        [ForeignKey(nameof(IdEstado))]
        public Estado? Estado { get; set; }

        // Descripción libre (nullable)
        public string? Descripcion { get; set; }

        // Fecha de creación: la fijaremos desde la NC
        public DateTime FechaCreacion { get; set; }

        // Fecha fin estimada (opcional)
        public DateTime? FechaFinEstimada { get; set; }
        public CorridaProduccion CorridaProduccion { get; set; }
    }
}
