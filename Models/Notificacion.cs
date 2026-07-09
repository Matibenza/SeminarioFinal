using System;
using System.ComponentModel.DataAnnotations;

namespace TesisPractica.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Mensaje { get; set; } = string.Empty; 

        public bool Leida { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Si querés asociarla a un usuario específico
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}
 