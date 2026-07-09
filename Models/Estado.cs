using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Estados")]
    public class Estado
    {
        [Key]
        public int IdEstado { get; set; }

        [Required, MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

    
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
        public ICollection<FichaTecnica> FichasTecnicas { get; set; }
           = new List<FichaTecnica>();
    }
}
