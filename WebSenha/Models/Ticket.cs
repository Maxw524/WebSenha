using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSenha.Models
{
    public class Ticket
    {
        // Identificador único do ticket
        [Key]
        public int Id { get; set; }

        // Número do ticket
        [Required(ErrorMessage = "O número do ticket é obrigatório.")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "O número do ticket deve ter entre 3 e 10 caracteres.")]
        public string Number { get; set; } = string.Empty;

        // Data de emissão do ticket
        [Required(ErrorMessage = "A data de emissão é obrigatória.")]
        public DateTime IssuedAt { get; set; }

        // Data em que o ticket foi chamado
        public DateTime? CalledAt { get; set; }

        // ID do serviço relacionado
        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        // Painel associado ao ticket
        public Painel? Painel { get; set; }

        // Status do ticket
        [Required(ErrorMessage = "O status do ticket é obrigatório.")]
        public TicketStatus Status { get; set; }
    }

    // Enum que representa os diferentes status de um ticket
    public enum TicketStatus
    {
        Waiting,   // Esperando
        Called,    // Chamado
        Served,    // Servido
        Cancelled  // Cancelado
    }
}
