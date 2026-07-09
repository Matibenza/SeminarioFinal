using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("NoConformidad")]
    public class NoConformidad
    {
        [Key]
        public int IdNoConformidad { get; set; }

        public DateTime FechaIncidente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaProduccion { get; set; }
        public int? Frecuencia { get; set; }
        public bool Recurrencia { get; set; }
        public string? Gravedad { get; set; }

        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        public int IdPieza { get; set; }
        public Pieza Pieza { get; set; }

        public int? Cantidad { get; set; }
        public string? Consecuencia { get; set; }

        public int IdProceso { get; set; }
        public Proceso Proceso { get; set; }

        public int IdDetectabilidad { get; set; }
        [ForeignKey(nameof(IdDetectabilidad))]
        public Detectabilidad Detectabilidad { get; set; }

        public int IdEstado { get; set; }
        public Estado Estado { get; set; }

        public int IdPrioridad { get; set; }
        public Prioridad Prioridad { get; set; }

        public DateTime? FechaFinalizacion { get; set; }
        public string? Descripcion { get; set; }

        // Relación N-a-N con Causa_NC y Defecto_NC
        public ICollection<Causa_NC> CausasNC { get; set; }
        public ICollection<Defecto_NC> DefectosNC { get; set; }

        public NoConformidad()
        {
            CausasNC = new List<Causa_NC>();
            DefectosNC = new List<Defecto_NC>();
        }

        // Relación 1-a-1 → 1 NC tiene 1 ficha técnica
   public FichaTecnica FichaTecnica { get; set; }
    }
}
