using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace WebSenha.Models
{
    public class Ticket
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("Numero")]
        [Display(Name = "Número")]
        [Required]
        public int Number { get; set; }

        [Column("EmitidoEm")]
        [Display(Name = "Emitido em")]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Column("Status")]
        [Display(Name = "Status")]
        [Required]
        public TicketStatus Status { get; set; }

        [Column("ChamadoEm")]
        [Display(Name = "Chamado em")]
        public DateTime? CalledAt { get; set; }

        [Column("PainelId")]
        public int PainelId { get; set; }
        public Painel Painel { get; set; }

        [Column("Tipo")]
        [Display(Name = "Tipo de Atendimento")]
        [Required]
        public TicketTipo Tipo { get; set; }

        // Relacionamento com a entidade Guiche
        [ForeignKey("GuicheId")]
        public Guiche Guiche { get; set; }  // Propriedade de navegação

        public int? GuicheId { get; set; }  // Chave estrangeira para Guiche (opcional, pode ser null)

        // Método para chamar o ticket
        public void ChamarTicket()
        {
            if (this.Status == TicketStatus.EmEspera)
            {
                this.Status = TicketStatus.Chamado;
                this.CalledAt = DateTime.UtcNow;
            }
        }
    }

    // Enum para o status do ticket
    public enum TicketStatus
    {
        Pendente,
        EmEspera,
        Chamado,
        Atendido,
        Cancelado
    }

    // Enum para o tipo de atendimento (normal ou preferencial)
    public enum TicketTipo
    {
        Normal,
        Preferencial
    }

    // Classe Guiche (exemplo)
    public class Guiche
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("Nome")]
        public string Nome { get; set; } // Nome do guichê
    }
}
