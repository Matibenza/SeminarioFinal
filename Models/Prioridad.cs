using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Prioridad")]
    public class Prioridad
    {
        [Key]
        public int IdPrioridad { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        // Colección de Tareas (inicializada para evitar null refs)
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
