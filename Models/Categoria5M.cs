using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    [Table("Categorias5M")]            
    public class Categoria5M
    {
        [Key]
        [Column("IdCategoria5M")]
        public int IdCategoria5M { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Descripcion")]
        public string Descripcion { get; set; }
    }
}
