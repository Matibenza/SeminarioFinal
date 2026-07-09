using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("VerificacionAcciones")]
    public class VerificacionAccion
    {
        [Key]
        public int IdVerificacion { get; set; }

        // Relación 1:1 con Tarea
        [Required]
        public int IdTarea { get; set; }

        [ForeignKey(nameof(IdTarea))]
        public Tarea? Tarea { get; set; }

        public bool EsEfectiva { get; set; }

        [StringLength(250)]
        public string? MetodoConfirmacion { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaVerificacion { get; set; }
    }
}
