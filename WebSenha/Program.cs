using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSenha.Data;
using WebSenha.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebSenha.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Configuração para escutar em todos os IPs nas portas 5000 e 6766
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Escuta em todos os IPs na porta 5000
    serverOptions.ListenAnyIP(5000); // Porta 5000

    // Escuta em todos os IPs na porta 6766
    serverOptions.ListenAnyIP(6766); // Porta 6766
});


// Adiciona suporte a APIs e controllers com views
builder.Services.AddEndpointsApiExplorer();  // Swagger
builder.Services.AddSwaggerGen();            // Swagger
builder.Services.AddControllersWithViews();  // Suporte a Controllers e Views
builder.Services.AddSignalR(); // Adiciona suporte ao SignalR

// Configuração do DbContext com a string de conexão do banco de dados
builder.Services.AddDbContext<QueueContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SistemaSenha")));

// Registra o TokenService no DI (injeção de dependência)
builder.Services.AddScoped<ITokenService, TokenService>();  // Adicionando o serviço para gerar senhas

// Configuração do CORS (origens permitidas)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
        builder.WithOrigins("http://localhost:3000", "http://192.168.1.100:6766") // Adicione o IP da sua rede local ou qualquer origem necessária
               .AllowAnyMethod()
               .AllowAnyHeader());
});

var app = builder.Build();

// Middleware para tratamento global de exceções
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception != null)
        {
            await context.Response.WriteAsync(new
            {
                message = "Ocorreu um erro inesperado. Tente novamente mais tarde.",
                details = exception.Message
            }.ToString());
        }
    });
});
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = {
            [".mp3"] = "audio/mpeg",
            [".ogg"] = "audio/ogg",
            [".wav"] = "audio/wav"
        }
    }
});

// Configuração do Pipeline de requisições HTTP
app.UseStaticFiles();  // Serve arquivos estáticos (como CSS, JS, etc.)
app.UseRouting();      // Habilita o roteamento de URLs
app.UseCors("AllowSpecificOrigins");  // Aplica a política CORS

// Desabilita o redirecionamento para HTTPS (caso você não tenha SSL configurado)
//app.UseHttpsRedirection();  // Redireciona HTTP para HTTPS - Desabilite se não usar SSL

app.UseAuthorization();  // Habilita a autorização

// Mapear o Hub SignalR
app.MapHub<SenhaHub>("/senhaHub"); // Adicionando o SignalR para comunicação em tempo real

// Configurações de rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicializa os dados, como tipos de senha, se necessário
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<QueueContext>();

    // Aplica as migrações pendentes
    context.Database.Migrate();

    // Inicializa os dados padrão (tipos de senha, painéis, tickets, etc.)
    DbInitializer.Initialize(services);
}

// Inicializa a aplicação
app.Run();

// Endpoint para criar um painel
app.MapPost("/painels", async (Painel painel, QueueContext db) =>
{
    if (painel == null || string.IsNullOrEmpty(painel.Senha))
    {
        return Results.BadRequest("Painel não pode ser nulo ou sem senha.");
    }

    var painelExistente = await db.Painels.AnyAsync(p => p.Senha == painel.Senha);
    if (painelExistente)
    {
        return Results.BadRequest("Já existe um painel com esta senha.");
    }

    db.Painels.Add(painel);
    await db.SaveChangesAsync();

    return Results.Created($"/painels/{painel.Id}", painel);
});

// Endpoint para listar painéis
app.MapGet("/painels", async (QueueContext db) =>
{
    var painels = await db.Painels.ToListAsync();
    return Results.Ok(painels);
});

// Endpoint para atualizar painel
app.MapPut("/painels/{id}", async (int id, Painel updatedPainel, QueueContext db) =>
{
    var painel = await db.Painels.FindAsync(id);
    if (painel == null) return Results.NotFound();

    painel.Senha = updatedPainel.Senha;
    painel.Guiche = updatedPainel.Guiche;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// Endpoint para deletar painel
app.MapDelete("/painels/{id}", async (int id, QueueContext db) =>
{
    var painel = await db.Painels.FindAsync(id);
    if (painel == null) return Results.NotFound();

    db.Painels.Remove(painel);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Endpoint para gerar uma senha
app.MapPost("/painels/gerar", (int tipoAtendimento, string service, ITokenService tokenService) =>
{
    if (tipoAtendimento <= 0 || string.IsNullOrEmpty(service))
    {
        return Results.BadRequest("Tipo de atendimento e serviço devem ser fornecidos.");
    }

    var senha = tokenService.GerarNovaSenha(tipoAtendimento, service); // Passando os dois parâmetros necessários
    return Results.Ok(new { senha });
});

// Endpoint para listar tickets
app.MapGet("/tickets", async (QueueContext db) =>
{
    var tickets = await db.Tickets.ToListAsync();
    return Results.Ok(tickets);
});

/// Endpoint para criar um ticket
app.MapPost("/tickets", async (Ticket ticket, QueueContext db) =>
{
    if (ticket.PainelId <= 0)
    {
        return Results.BadRequest("O PainelId deve ser fornecido.");
    }

    // Se o tipo não for fornecido, o valor padrão será 'Normal'
    if (ticket.Tipo == TicketTipo.Normal || ticket.Tipo == TicketTipo.Preferencial)
    {
        var lastTicket = await db.Tickets.OrderByDescending(t => t.Number).FirstOrDefaultAsync();
        int nextTicketNumber = (lastTicket == null) ? 1 : lastTicket.Number + 1;

        var newTicket = new Ticket
        {
            Number = nextTicketNumber,
            Status = TicketStatus.EmEspera,  // A senha inicia em "Em Espera"
            Tipo = ticket.Tipo,  // Definindo o tipo do ticket
            PainelId = ticket.PainelId
        };

        db.Tickets.Add(newTicket);
        await db.SaveChangesAsync();

        return Results.Created($"/tickets/{newTicket.Id}", newTicket);
    }
    else
    {
        return Results.BadRequest("Tipo de ticket inválido.");
    }
});


// Endpoint para chamar um ticket
app.MapPut("/tickets/call/{id}", async (int id, QueueContext db, IHubContext<SenhaHub> hubContext) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket == null)
    {
        return Results.NotFound("Ticket não encontrado.");
    }

    // Verificar se a senha está em espera
    if (ticket.Status != TicketStatus.EmEspera)
    {
        return Results.BadRequest("A senha não está mais em espera.");
    }

    // Atualiza o status da senha para "Chamado"
    ticket.Status = TicketStatus.Chamado;
    ticket.CalledAt = DateTime.Now;

    // A alteração do tipo está sendo corretamente persistida ou não?
    // Verifique se o tipo está correto antes de salvar.
    // Se necessário, insira a lógica para garantir que o tipo esteja correto.
    // ticket.Tipo = algumTipo;

    await db.SaveChangesAsync();

    // Notifica todos os clientes conectados sobre a nova senha chamada
    await hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", new
    {
        ticket.Number,
        Tipo = ticket.Tipo.ToString(),  // Converte o tipo para string
        Guiche = ticket.Guiche?.Nome ?? "Não atribuído"
    });

    return Results.Ok(ticket);  // Retorna o ticket chamado
});

// Endpoint para exibir o próximo ticket na TV (sem chamá-lo ainda)
app.MapPut("/painels/{painelId}/next-ticket", async (int painelId, QueueContext db) =>
{
    var painel = await db.Painels.FindAsync(painelId);
    if (painel == null)
    {
        return Results.NotFound("Painel não encontrado.");
    }

    // Pega o próximo ticket que está "Em Espera" e ordena pela data de emissão
    var nextTicket = await db.Tickets
        .Where(t => t.PainelId == painelId && t.Status == TicketStatus.EmEspera)
        .OrderBy(t => t.IssuedAt)  // Garante a ordem de chegada
        .FirstOrDefaultAsync();

    if (nextTicket == null)
    {
        return Results.NotFound("Nenhum ticket em espera para esse painel.");
    }

    // Exibe o ticket na TV, mas sem alterá-lo para "Chamado"
    return Results.Ok(nextTicket);  // Apenas exibe o ticket na TV, sem marcar como chamado
});

// Endpoint para listar tickets por status para um painel
app.MapGet("/painels/{painelId}/tickets", async (int painelId, TicketStatus status, QueueContext db) =>
{
    var tickets = await db.Tickets
        .Where(t => t.PainelId == painelId && t.Status == status)
        .ToListAsync();

    if (!tickets.Any())
    {
        return Results.NotFound($"Nenhum ticket com status {status} para o painel {painelId}.");
    }

    return Results.Ok(tickets);
});
