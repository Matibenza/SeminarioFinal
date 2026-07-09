using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Paso3")]
    public class Paso3
    {
        [Key, ForeignKey(nameof(FichaTecnica))]
        public int IdFichaTecnica { get; set; }

        public FichaTecnica? FichaTecnica { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaFinEstimada { get; set; }

        [Required]
        public int Responsable { get; set; }

        [Required]
        public int IdEstado { get; set; }

    }
}
