using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSenha.Models
{
    [Table("Painel")]
    public class Painel
    {
        [Key]
        [Column("id")]
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Column("Senha")] 
        [Display(Name = "Senha")]
        [Required]
        [StringLength(100)]
        public string Senha { get; set; }

        [Column("Guiche")]
        [Display(Name = "Guiche")]
        [StringLength(50)]
        public string Guiche { get; set; }
    }
}
