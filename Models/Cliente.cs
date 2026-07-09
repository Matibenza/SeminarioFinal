using System.ComponentModel.DataAnnotations;

public class Cliente
{
    [Key]
    public int IdCliente { get; set; }  // La PK en la tabla Cliente

    public string? Nombre { get; set; }

    // Navegación inversa
    public ICollection<Pieza>? Piezas { get; set; }
}
