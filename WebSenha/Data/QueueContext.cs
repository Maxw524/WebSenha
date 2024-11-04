using Microsoft.EntityFrameworkCore;
using WebSenha.Models;

namespace WebSenha.Data // Atualizado para o novo nome do projeto
{
    public class QueueContext : DbContext
    {
        public QueueContext(DbContextOptions<QueueContext> options) : base(options) { }

        public DbSet<Painel> Painels { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Painel>(entity =>
            {
                entity.ToTable("Painel");

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.Senha)
                    .IsRequired() // Define como obrigatório
                    .HasMaxLength(100) // Define o tamanho máximo
                    .HasColumnName("Senha");

                entity.Property(e => e.Guiche)
                    .HasMaxLength(50) // Define o tamanho máximo
                    .HasColumnName("Guiche");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                // Configure as propriedades do Ticket aqui, se necessário
                // Exemplo:
                // entity.ToTable("Tickets");
                // entity.Property(e => e.Id).HasColumnName("id");
                // ... (outras configurações)
            });
        }
    }
}