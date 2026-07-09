using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Paso2")]
    public class Paso2AnalisisCausa
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


        // Relación 1 a N -> CausasPasos
        public ICollection<CausaPaso> CausasPasos { get; set; } = new List<CausaPaso>();
    }
}
