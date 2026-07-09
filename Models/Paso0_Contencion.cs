using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TesisPractica.Models
{
    [Table("Paso0_Contencion")]
    public class Paso0Contencion
    {
        // PK y FK 1-1 a FichasTecnicas.IdFichaTecnica
        [Key, ForeignKey(nameof(FichaTecnica))]
        public int IdFichaTecnica { get; set; }

        // Estado del paso (FK a Estados)
        public int IdEstado { get; set; }
        [BindNever, ValidateNever]
        [ForeignKey(nameof(IdEstado))]
        public Estado Estado { get; set; }

        // Datos de control comunes
        public DateTime FechaCreacion { get; set; }

        public int Responsable { get; set; }     // aquí guardas Usuario.Id
        [BindNever, ValidateNever]
        [ForeignKey(nameof(Responsable))]
        public Usuario ResponsableUsuario { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinEstimada { get; set; }
        public DateTime? FechaFin { get; set; }

        // Campos específicos de la contención
        [MaxLength(150)]
        public string? AccionContencion { get; set; }

        [MaxLength(100)]
        public string? MetodoControl { get; set; }

        public int? Deposito_CantControlada { get; set; }
        public int? Deposito_CantSospechosa { get; set; }
        public int? Deposito_CantOk { get; set; }

        public int? Almacen_CantControlada { get; set; }
        public int? Almacen_CantSospechosa { get; set; }
        public int? Almacen_CantOk { get; set; }

        public int? BordeLinea_CantControlada { get; set; }
        public int? BordeLinea_CantSospechosa { get; set; }
        public int? BordeLinea_CantOk { get; set; }

        // Navegación 1-a-1 hacia la ficha técnica
        [BindNever, ValidateNever]
        public FichaTecnica FichaTecnica { get; set; }
    }
}
