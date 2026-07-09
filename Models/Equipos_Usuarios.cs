using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Equipos_Usuarios")]
    public class Equipos_Usuarios
    {
        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Usuario))]
        public int IdUsuario { get; set; }

        [Key, Column(Order = 2)]
        [ForeignKey(nameof(Equipo))]
        public int IdEquipo { get; set; }

        public Usuario Usuario { get; set; }
        public Equipo Equipo { get; set; }

        public string RolEnEquipo { get; set; } // Ej: "Piloto", "Auditor", "Observador"
    }
}
