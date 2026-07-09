using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Defecto_NC")]
    public class Defecto_NC
    {
        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Defecto))]
        public int IdDefecto { get; set; }

        [Key, Column(Order = 2)]
        [ForeignKey(nameof(NoConformidad))]
        public int IdNoConformidad { get; set; }

        public Defecto Defecto { get; set; }
        public NoConformidad NoConformidad { get; set; }
    }
}
