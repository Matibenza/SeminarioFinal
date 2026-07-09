using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("CausasPasos")]
    public class CausaPaso
    {
        // Clave primaria compuesta
        [Key, Column(Order = 0)]
        public int IdFichaTecnica { get; set; }
        [Key, Column(Order = 1)]
        public int IdCausa { get; set; }

        // FK a Paso2AnalisisCausa (opcional)
        [ForeignKey(nameof(IdFichaTecnica))]
        public Paso2AnalisisCausa? Paso2 { get; set; }

        // Categoría 5M (obligatorio)
        [Required]
        public int IdCategoria5M { get; set; }
        [ForeignKey(nameof(IdCategoria5M))]
        [ValidateNever]     // Evita validación de datos
        [BindNever]         // Evita binding por parte del model binder
        public Categoria5M Categoria5M { get; set; }

        // Descripción de la causa (opcional, max 200)
        [MaxLength(200)]
        public string? DescripcionCausa { get; set; }

        // Resultados y clasificaciones (opcionales)
        public short? ResultadoVerificacion { get; set; }
        public short? ClasificacionImpacto { get; set; }
        public short? EsCausaRaiz { get; set; }

        // Relación 1:1 a Analisis5Porque
        public Analisis5Porque? Analisis5Porque { get; set; }

        // Tareas asociadas (inicializada para evitar null refs)
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
