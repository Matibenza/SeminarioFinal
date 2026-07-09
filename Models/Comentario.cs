using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TesisPractica.Models;

[Table("Comentarios")]
public class Comentario
{
    [Key]
    public int IdComentario { get; set; }

    [Required, MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    // FK obligatoria a Tarea
    public int IdTarea { get; set; }
    [ForeignKey(nameof(IdTarea))]
    public Tarea Tarea { get; set; }

    // <-- estos tres faltaban:
    public int UsuarioId { get; set; }
    [ForeignKey(nameof(UsuarioId))]
    public Usuario Usuario { get; set; }

    public DateTime FechaCreacion { get; set; }
}
