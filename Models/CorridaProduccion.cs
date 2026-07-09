using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("CorridaProduccion")]
    public class CorridaProduccion
    {
        [Key]
        public int IdProduccion { get; set; }

        // FK y relación 1:1 con FichaTecnica
        [Required]
        public int IdFichaTecnica { get; set; }

        [ForeignKey(nameof(IdFichaTecnica))]
        [BindNever]
        [ValidateNever]
        public FichaTecnica FichaTecnica { get; set; }

        // Datos de producción
        public int CantidadProducida { get; set; }
        public int CantidadOK { get; set; }
        public int CantidadNoOK { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaProduccion { get; set; }
    }
}
