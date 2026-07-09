using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Causa_NC")]
    public class Causa_NC
    {
        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Causa))]
        public int IdCausa { get; set; }

        [Key, Column(Order = 2)]
        [ForeignKey(nameof(NoConformidad))]
        public int IdNoConformidad { get; set; }

        // Navegación
        public Causa Causa { get; set; }
        public NoConformidad NoConformidad { get; set; }
    }
}
