namespace TesisPractica.Models;
using System.ComponentModel.DataAnnotations;

public class Proceso
{
    [Key]
    public int IdProcesos { get; set; }
    public string Nombre { get; set; }
}
