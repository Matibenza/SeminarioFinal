using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Paso1")]
    public class Paso1Verificacion
    {
        [Key]
        [ForeignKey(nameof(FichaTecnica))]
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

        public short? TipoProblema { get; set; }
        public short? Turno { get; set; }

        [MaxLength(200)]
        public string? Objetivo { get; set; }

        [MaxLength(100)]
        public string? QueSucede { get; set; }

        [MaxLength(100)]
        public string? QuienDetecta { get; set; }

        public DateTime? CuandoSucede { get; set; }

        [MaxLength(100)]
        public string? ComoSucede { get; set; }

        [MaxLength(100)]
        public string? CualPieza { get; set; }

        public int? IdOperador { get; set; }
    }
}
