using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TesisPractica.Models;

public class Pieza
{
    [Key]
    public int IdPieza { get; set; }

    // Indicar que este campo es la FK hacia Cliente
    [ForeignKey(nameof(Cliente))]
    public int? IdCliente { get; set; }

    // Navegación
    public Cliente? Cliente { get; set; }

    public string Descripcion { get; set; }

    // Ejemplo de relación con NoConformidad
    public ICollection<NoConformidad>? NoConformidades { get; set; }
}
