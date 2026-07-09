using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TesisPractica.Models
{
    public class Paso6
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
    }
}
