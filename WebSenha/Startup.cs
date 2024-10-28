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

        // M�todo para configurar os servi�os da aplica��o
        public void ConfigureServices(IServiceCollection services)
        {
            // Configura��o do DbContext para usar o banco de dados "SistemaSenha"
            services.AddDbContext<Contexto>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SistemaSenha")));

            // Adiciona suporte a controladores e views
            services.AddControllersWithViews();
        }

        // M�todo para configurar o pipeline de requisi��es HTTP
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configura��o para ambientes de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // P�gina de erro em ambientes de produ��o
                app.UseExceptionHandler("/Home/Error");
            }

            // Habilita arquivos est�ticos
            app.UseStaticFiles();

            // Habilita o roteamento
            app.UseRouting();

            // Habilita autoriza��o
            app.UseAuthorization();

            // Configura��o das rotas da aplica��o
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
