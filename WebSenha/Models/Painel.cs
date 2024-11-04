using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSenha.Models
{
    // Modelo que representa um painel de senhas
    [Table("Painel")]
    public class Painel
    {
        // Identificador único do painel
        [Key]
        [Column("id")]
        [Display(Name = "ID")]
        public int Id { get; set; }

        // Senha gerada
        [Column("Senha")]
        [Display(Name = "Senha")]
        [Required] // A senha é obrigatória
        [StringLength(100)] // Limite de caracteres
        public string Senha { get; set; }

        // Guichê associado
        [Column("Guiche")]
        [Display(Name = "Guiche")]
        [StringLength(50)] // Limite de caracteres
        public string Guiche { get; set; }

        // Coleção de tickets associados ao painel
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}