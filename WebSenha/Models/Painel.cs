using System;
using System.Collections.Generic;
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

        // Alterado para permitir valores maiores para o Tipo de Senha (como "Atendimento" e "Caixa")
        [Column("Tipo")]
        [Display(Name = "Tipo de Senha")]
        [Required]
        [StringLength(50)]  // Ajustado para permitir valores maiores
        public string Tipo { get; set; }

        [Column("CriadoEm")]
        [Display(Name = "Criado em")]
        public DateTime CriadoEm { get; set; }

        // Usando o enum StatusSenha já definido em StatusSenha.cs
        [Column("Status")]
        [Display(Name = "Status")]
        public StatusSenha Status { get; set; }

        // Adicionando a nova propriedade Service
        [Column("Service")]
        [Display(Name = "Serviço")]
        [StringLength(50)]
        public string Service { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public Painel()
        {
            CriadoEm = DateTime.Now; // Define a data e hora da criação
            Status = StatusSenha.Pendente; // Status padrão "Pendente"
        }
    }
}
