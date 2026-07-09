using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Equipos")]
    public class Equipo
    {
        [Key]
        public int IdEquipo { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // 🔗 Clave foránea al usuario que creó el equipo
        public int IdCreador { get; set; }
        public DateTime FechaCreacion { get; set; }

        [ForeignKey("IdCreador")]
        public Usuario Creador { get; set; }  // navegación opcional

        // ✅ Única propiedad de navegación a Equipos_Usuarios
        public ICollection<Equipos_Usuarios> EquiposUsuarios { get; set; } = new List<Equipos_Usuarios>();

        // Un equipo puede tener muchas fichas técnicas
   public ICollection<FichaTecnica> FichasTecnicas { get; set; } = new List<FichaTecnica>();
    }
}
