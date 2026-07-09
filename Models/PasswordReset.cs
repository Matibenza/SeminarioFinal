using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisPractica.Models
{
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Usuario))]
        public int UserId { get; set; }

        public Usuario Usuario { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime? Expiration { get; set; }
    }
}
