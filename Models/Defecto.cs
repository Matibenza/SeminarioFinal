namespace TesisPractica.Models;
using System.ComponentModel.DataAnnotations;

public class Defecto
{
    [Key]
    public int IdDefecto { get; set; }
    public string NombreDefecto { get; set; }

    // Si querés la relación inversa (opcional)
    public ICollection<Defecto_NC> DefectosNC { get; set; }
}
