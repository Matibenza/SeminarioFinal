namespace TesisPractica.Models;
using System.ComponentModel.DataAnnotations;

public class Causa
{
    [Key]
    public int IdCausa { get; set; }
    public string Descripcion { get; set; }

    // Relación inversa con la tabla puente Causa_NC (opcional)
    public ICollection<Causa_NC> CausasNC { get; set; }
}
