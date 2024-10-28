using Microsoft.EntityFrameworkCore;
using WebSenha.Models;

namespace WebSenha.Config
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {
           
        }

        public DbSet<Painel> Painel { get; set; }
    }
}
