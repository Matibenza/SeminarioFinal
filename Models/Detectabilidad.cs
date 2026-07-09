using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Detectabilidad")]
    public class Detectabilidad
    {
        [Key]
        public int IdDetectabilidad { get; set; }

        [Required]
        public string Descripcion { get; set; }

        // Una detectabilidad puede estar asociada a varias no conformidades
        public ICollection<NoConformidad> NoConformidades { get; set; }
    }
}
