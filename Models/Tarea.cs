using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;  // <-- para BindNever

namespace TesisPractica.Models
{
    [Table("Tareas")]
    public class Tarea
    {
        [Key]
        public int IdTarea { get; set; }

        [Required]
        public int IdFichaTecnica { get; set; }

        [BindNever]
        [ForeignKey(nameof(IdFichaTecnica))]
        public FichaTecnica? FichaTecnica { get; set; }

        [Required]
        public int IdTipoAccion { get; set; }

        [BindNever]
        [ForeignKey(nameof(IdTipoAccion))]
        public TipoAccion? TipoAccion { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaObjetivo { get; set; }

        [Column(TypeName = "date")]
        public DateTime FechaFinal { get; set; }

        [Required]
        public int IdPrioridad { get; set; }

        [BindNever]
        [ForeignKey(nameof(IdPrioridad))]
        public Prioridad? Prioridad { get; set; }

        [Required]
        public int IdResponsable { get; set; }

        [BindNever]
        [ForeignKey(nameof(IdResponsable))]
        public Usuario? Responsable { get; set; }

        [Required]
        public int IdEstado { get; set; }

        [BindNever]
        [ForeignKey(nameof(IdEstado))]
        public Estado? Estado { get; set; }

        [Required]
        public int IdCausaPasos { get; set; }

        [BindNever]
        [ForeignKey(nameof(IdCausaPasos))]
        public CausaPaso? CausaPaso { get; set; }

        // Campos de texto libre (opcionales)
        public string? AccionDeMejora { get; set; }
        public string? DescripcionDeMejora { get; set; }
        public string? LugarAplicacion { get; set; }
        public string? CanalImplementacion { get; set; }

        // Comentarios asociados
        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public VerificacionAccion? VerificacionAccion { get; set; }
    }
}
