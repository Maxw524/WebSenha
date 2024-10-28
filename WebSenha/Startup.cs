using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSenha.Config;

namespace WebSenha
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Método para configurar os serviços da aplicação
        public void ConfigureServices(IServiceCollection services)
        {
            // Configuração do DbContext para usar o banco de dados "SistemaSenha"
            services.AddDbContext<Contexto>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SistemaSenha")));

            // Adiciona suporte a controladores e views
            services.AddControllersWithViews();
        }

        // Método para configurar o pipeline de requisições HTTP
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configuração para ambientes de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Página de erro em ambientes de produção
                app.UseExceptionHandler("/Home/Error");
            }

            // Habilita arquivos estáticos
            app.UseStaticFiles();

            // Habilita o roteamento
            app.UseRouting();

            // Habilita autorização
            app.UseAuthorization();

            // Configuração das rotas da aplicação
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
