using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSenha.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O número do ticket é obrigatório.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "O número do ticket deve ter entre 3 e 10 caracteres.")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de emissão é obrigatória.")]
        public DateTime IssuedAt { get; set; }

        public DateTime? CalledAt { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        // Adicionando a propriedade PainelId
        [ForeignKey("Painel")]
        public int PainelId { get; set; } // Adiciona a propriedade PainelId

        public Painel Painel { get; set; }

        [Required(ErrorMessage = "O status do ticket é obrigatório.")]
        public TicketStatus Status { get; set; }
    }

    public enum TicketStatus
    {
        Waiting,
        Called,
        Served,
        Cancelled
    }
}