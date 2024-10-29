using Microsoft.EntityFrameworkCore;
using WebSenha.Models; // Certifique-se de que este é o namespace correto

namespace WebSenha.Data // Certifique-se de que este é o novo namespace
{
    public class QueueContext : DbContext
    {
        public QueueContext(DbContextOptions<QueueContext> options) : base(options) { }

        // DbSet para Painéis
        public DbSet<Painel> Painels { get; set; } // O nome foi alterado para o plural para seguir as convenções

        // DbSet para Tickets
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações adicionais podem ser feitas aqui
            // Exemplo: modelBuilder.Entity<Painel>().ToTable("Painels");
            // Adicione qualquer configuração específica para suas entidades aqui
        }
    }
}
