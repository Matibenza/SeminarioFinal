using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    public class Paso5
    {
        [Key, ForeignKey(nameof(FichaTecnica))]
        public int IdFichaTecnica { get; set; }

        public FichaTecnica? FichaTecnica { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public DateTime? FechaFinEstimada { get; set; }

        [Required]
        public int Responsable { get; set; }

        [Required]
        public int IdEstado { get; set; }

        // ───────────── Campos de evaluación (opcionales) ─────────────
        public short? FacilidadSeguimiento { get; set; }

        public short? Instrucciones { get; set; }

        public short? EstandarCalidad { get; set; }

        public short? Defectos { get; set; }

        public short? ParametrosCeroDefectos { get; set; }

        public short? Variacion { get; set; }

        public short? VariacionTrabajo { get; set; }

        public short? LiberacionDefectos { get; set; }
    }
}
