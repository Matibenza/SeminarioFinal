using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Tipos_Acciones")]
    public class TipoAccion
    {
        [Key]
        public int IdTipoAccion { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        // Relación 1:N con Tarea
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
