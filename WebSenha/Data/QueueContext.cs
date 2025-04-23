using Microsoft.EntityFrameworkCore;
using WebSenha.Models;
using System;

namespace WebSenha.Data
{
    public class QueueContext : DbContext
    {
        public QueueContext(DbContextOptions<QueueContext> options) : base(options) { }

        // DbSet para as entidades
        public DbSet<Painel> Painels { get; set; }   // Tabela Painel (singular)
        public DbSet<TipoSenha> TiposSenhas { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração para a tabela Painel (singular)
            modelBuilder.Entity<Painel>(entity =>
            {
                entity.ToTable("Painel");  // Tabela Painel (singular)

                // Configuração da coluna 'Id' como chave primária
                entity.Property(e => e.Id)
                    .HasColumnName("id");

                // Configuração da coluna 'Senha' como obrigatória e com tamanho máximo de 100
                entity.Property(e => e.Senha)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Senha");

                // Configuração da coluna 'Guiche' com tamanho máximo de 50
                entity.Property(e => e.Guiche)
                    .HasMaxLength(50)
                    .HasColumnName("Guiche");

                // Configuração do campo 'Status' para armazenar o valor do Enum como inteiro
                entity.Property(e => e.Status)
                    .HasConversion<int>() // Converte o Enum StatusSenha para inteiro no banco
                    .HasColumnName("Status");

                // Relacionamento com a tabela Ticket (Um Painel tem muitos Tickets)
                entity.HasMany(p => p.Tickets)
                    .WithOne(t => t.Painel)
                    .HasForeignKey(t => t.PainelId)
                    .OnDelete(DeleteBehavior.Cascade);  // Deletar Tickets quando o Painel for deletado
            });

            // Configuração para a tabela TipoSenha
            modelBuilder.Entity<TipoSenha>(entity =>
            {
                entity.ToTable("TipoSenha");

                // Configuração da coluna 'Id' como chave primária
                entity.Property(e => e.Id)
                    .HasColumnName("id");

                // Configuração da coluna 'Descricao' como obrigatória e com tamanho máximo de 100
                entity.Property(e => e.Descricao)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("descricao");

                // Coluna 'CriadoEm' com valor padrão (data/hora atual)
                entity.Property(e => e.CriadoEm)
                    .HasColumnName("criado_em")
                    .HasDefaultValueSql("GETDATE()");

                // Coluna 'AtualizadoEm' com valor padrão (data/hora atual)
                entity.Property(e => e.AtualizadoEm)
                    .HasColumnName("atualizado_em")
                    .HasDefaultValueSql("GETDATE()");
            });

            // Configuração para a tabela Ticket
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket");

                // Configuração da coluna 'Id' como chave primária
                entity.Property(e => e.Id)
                    .HasColumnName("id");

                // Configuração da coluna 'PainelId' como chave estrangeira
                entity.Property(e => e.PainelId)
                    .HasColumnName("PainelId");

                // Configuração da coluna 'Numero' como obrigatória
                entity.Property(e => e.Number)
                    .IsRequired()  // Define como obrigatório
                    .HasColumnName("Numero");

                // Configuração da coluna 'Status' para armazenar o valor da enumeração TicketStatus
                entity.Property(e => e.Status)
                    .HasConversion<string>()  // Alterado para armazenar como string (por exemplo: "Pendente", "Chamado", etc.)
                    .HasColumnName("Status");

                // Relacionamento com a tabela Painel (Cada Ticket pertence a um Painel)
                entity.HasOne(t => t.Painel)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(t => t.PainelId)
                    .OnDelete(DeleteBehavior.Cascade);  // Deletar Tickets quando o Painel for deletado
            });
        }
    }
}
