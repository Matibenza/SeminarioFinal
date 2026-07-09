using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Analisis5Porque")]
    public class Analisis5Porque
    {
        [Key]
        public int IdAnalisis { get; set; }

        // Clave foránea compuesta: ambas columnas deben coincidir con CausaPaso
        public int IdFichaTecnica { get; set; }
        public int IdCausa { get; set; }

        public string? PrimerPorque { get; set; }
        public string? SegundoPorque { get; set; }
        public string? TercerPorque { get; set; }
        public string? CuartoPorque { get; set; }
        public string? QuintoPorque { get; set; }

        // Propiedad de navegación
        public CausaPaso CausaPaso { get; set; } = null!;
    }
}
