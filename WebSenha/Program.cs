using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSenha.Data; 
using WebSenha.Models;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os ao cont�iner.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews(); // Adiciona suporte para controladores e views

// Configura o DbContext com a string de conex�o para SQL Server
builder.Services.AddDbContext<QueueContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SistemaSenha"))); // Use SQL Server aqui

// Adiciona suporte ao CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

// Configura o pipeline de requisi��es HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Para ver mensagens de erro no desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // P�gina de erro gen�rica
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Para servir arquivos est�ticos de wwwroot
app.UseRouting();

app.UseCors("AllowAllOrigins"); // Usa a pol�tica de CORS
app.UseAuthorization();

// Mapeia as rotas padr�o para o MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Aqui, voc� pode adicionar os endpoints restantes para sua API
// Endpoint para criar um painel
app.MapPost("/painels", async (Painel painel, QueueContext db) =>
{
    if (painel is null)
    {
        return Results.BadRequest("Painel n�o pode ser nulo.");
    }

    db.Painels.Add(painel);
    await db.SaveChangesAsync();

    return Results.Created($"/painels/{painel.Id}", painel);
});

// Endpoint para listar pain�is
app.MapGet("/painels", async (QueueContext db) =>
{
    var painels = await db.Painels.ToListAsync();
    return Results.Ok(painels);
});

// Endpoint para atualizar um painel
app.MapPut("/painels/{id}", async (int id, Painel updatedPainel, QueueContext db) =>
{
    var painel = await db.Painels.FindAsync(id);
    if (painel is null) return Results.NotFound();

    painel.Senha = updatedPainel.Senha; // Atualiza a senha do painel
    painel.Guiche = updatedPainel.Guiche; // Atualiza o guich�
    await db.SaveChangesAsync();

    return Results.NoContent(); // Retorna 204 No Content
});

// Endpoint para deletar um painel
app.MapDelete("/painels/{id}", async (int id, QueueContext db) =>
{
    var painel = await db.Painels.FindAsync(id);
    if (painel is null) return Results.NotFound();

    db.Painels.Remove(painel);
    await db.SaveChangesAsync();
    return Results.NoContent(); // Retorna 204 No Content
});

// Endpoint para criar um ticket com n�mero autom�tico
app.MapPost("/tickets", async (Ticket ticket, QueueContext db) =>
{
    if (ticket.PainelId <= 0) // Verifica se o PainelId � v�lido
    {
        return Results.BadRequest("O PainelId deve ser fornecido.");
    }

    // Busca o �ltimo ticket criado
    var lastTicket = await db.Tickets.OrderByDescending(t => t.Id).FirstOrDefaultAsync();

    // Gera o pr�ximo n�mero sequencial
    var nextTicketNumber = lastTicket == null ? "0001" : (int.Parse(lastTicket.Number) + 1).ToString("D4");

    var newTicket = new Ticket
    {
        Number = nextTicketNumber,
        IssuedAt = DateTime.Now,
        Status = TicketStatus.Waiting,
        PainelId = ticket.PainelId // Adiciona o PainelId ao ticket
    };

    db.Tickets.Add(newTicket);
    await db.SaveChangesAsync();

    return Results.Created($"/tickets/{newTicket.Id}", newTicket);
});

// Endpoint para listar tickets
app.MapGet("/tickets", async (QueueContext db) =>
{
    var tickets = await db.Tickets.ToListAsync();
    return Results.Ok(tickets);
});

// Endpoint para chamar um ticket
app.MapPut("/tickets/call/{id}", async (int id, QueueContext db) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();

    ticket.Status = TicketStatus.Called; // Atualiza o status para "Called"
    ticket.CalledAt = DateTime.Now;      // Atualiza a hora de chamada do ticket
    await db.SaveChangesAsync();

    return Results.NoContent(); // Retorna 204 No Content
});

// Endpoint para cancelar um ticket
app.MapPut("/tickets/cancel/{id}", async (int id, QueueContext db) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();

    ticket.Status = TicketStatus.Cancelled; // Atualiza o status para "Cancelled"
    await db.SaveChangesAsync();

    return Results.NoContent(); // Retorna 204 No Content
});

// Endpoint para chamar o pr�ximo ticket de um painel
app.MapPut("/painels/{painelId}/next-ticket", async (int painelId, QueueContext db) =>
{
    var nextTicket = await db.Tickets
        .Where(t => t.PainelId == painelId && t.Status == TicketStatus.Waiting)
        .OrderBy(t => t.IssuedAt)
        .FirstOrDefaultAsync();

    if (nextTicket is null)
    {
        return Results.NotFound("Nenhum ticket em espera para esse painel.");
    }

    nextTicket.Status = TicketStatus.Called;
    nextTicket.CalledAt = DateTime.Now;

    await db.SaveChangesAsync();
    return Results.Ok(nextTicket);
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

app.Run();