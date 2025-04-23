using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WebSenha.Models;

namespace WebSenha.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new QueueContext(
                serviceProvider.GetRequiredService<DbContextOptions<QueueContext>>()))
            {
                // Verifica se já existem tipos de senha
                if (context.TiposSenhas.Any())
                {
                    return; // Já existem
                }

                // Adiciona tipos de senha padrão
                context.TiposSenhas.AddRange(
                    new TipoSenha { Descricao = "Preferencial" },
                    new TipoSenha { Descricao = "Normal" }
                );

                // Salva as alterações no banco de dados
                context.SaveChanges();

                // Verifica se existem painéis cadastrados. Caso contrário, adiciona alguns painéis.
                if (!context.Painels.Any())
                {
                    // Adiciona alguns painéis de exemplo
                    context.Painels.AddRange(
                        new Painel { Senha = "CX-001", Tipo = "P", Status = StatusSenha.Pendente, CriadoEm = DateTime.Now },
                        new Painel { Senha = "CX-002", Tipo = "N", Status = StatusSenha.Pendente, CriadoEm = DateTime.Now },
                        new Painel { Senha = "AT-001", Tipo = "P", Status = StatusSenha.Pendente, CriadoEm = DateTime.Now },
                        new Painel { Senha = "AT-002", Tipo = "N", Status = StatusSenha.Pendente, CriadoEm = DateTime.Now }
                    );

                    context.SaveChanges();
                }

                // Verifica se existem tickets cadastrados
                if (!context.Tickets.Any())
                {
                    // Adiciona alguns tickets de exemplo (deve estar associado ao painel)
                    var painel1 = context.Painels.FirstOrDefault(p => p.Senha == "CX-001");
                    var painel2 = context.Painels.FirstOrDefault(p => p.Senha == "AT-001");

                    if (painel1 != null)
                    {
                        context.Tickets.Add(new Ticket { Number = 1, Status = TicketStatus.EmEspera, PainelId = painel1.Id, IssuedAt = DateTime.Now });
                    }
                    if (painel2 != null)
                    {
                        context.Tickets.Add(new Ticket { Number = 1, Status = TicketStatus.EmEspera, PainelId = painel2.Id, IssuedAt = DateTime.Now });
                    }

                    context.SaveChanges();
                }
            }
        }
    }
}