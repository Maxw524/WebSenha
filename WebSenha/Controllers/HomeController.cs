using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using WebSenha.Data;
using WebSenha.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly QueueContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHubContext<SenhaHub> _hubContext;

    public HomeController(ILogger<HomeController> logger, QueueContext context, IWebHostEnvironment webHostEnvironment, IHubContext<SenhaHub> hubContext)
    {
        _logger = logger;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _hubContext = hubContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Token()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Método para retornar a última senha chamada (painel principal)
    public async Task<IActionResult> GetUltimaSenha()
    {
        try
        {
            _logger.LogInformation("Consultando a última senha chamada...");

            // Busca a senha mais recente com status 'Chamado'
            var ultimaSenha = await _context.Painels
                .Where(p => p.Status == StatusSenha.Chamado) // Apenas senhas chamadas
                .OrderByDescending(p => p.CriadoEm)         // Ordena pela data mais recente
                .FirstOrDefaultAsync();                     // Pega a última chamada

            if (ultimaSenha == null)
            {
                _logger.LogInformation("Nenhuma senha com status 'Chamado' encontrada.");
                return Json(new { senha = (string)null, status = (int?)null, guiche = (string)null });
            }

            // Retorna apenas a senha, o status e o guichê
            return Json(new
            {
                senha = ultimaSenha.Senha,
                status = ultimaSenha.Status,
                guiche = ultimaSenha.Guiche  // Incluindo o guichê
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao obter a última senha: {Mensagem}, Detalhes: {StackTrace}", ex.Message, ex.StackTrace);
            return StatusCode(500, "Erro interno do servidor.");
        }
    }


    // Método para retornar as últimas 3 senhas chamadas
    public async Task<IActionResult> GetSenhas()
    {
        try
        {
            _logger.LogInformation("Consultando as últimas 3 senhas chamadas...");

            // Busca as últimas 3 senhas chamadas com status 'Chamado'
            var ultimasSenhas = await _context.Painels
                .Where(p => p.Status == StatusSenha.Chamado)
                .OrderByDescending(p => p.CriadoEm) // Ordena pela data de criação
                .Take(3) // Pega as 3 mais recentes
                .ToListAsync(); // Retorna como lista

            if (ultimasSenhas == null || !ultimasSenhas.Any())
            {
                _logger.LogInformation("Nenhuma senha chamada encontrada.");
                return Json(new List<object>());
            }

            // Retorna as últimas 3 senhas chamadas em formato JSON
            var senhas = ultimasSenhas.Select(s => new
            {
                senha = s.Senha,
                tipo = s.Tipo,
                guiche = s.Guiche,
                status = s.Status
            }).ToList();

            return Json(senhas);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao obter as últimas senhas: {Mensagem}, Detalhes: {StackTrace}", ex.Message, ex.StackTrace);
            return StatusCode(500, "Erro interno do servidor.");
        }
    }

    // Método para chamar um ticket
    public async Task<IActionResult> ChamarTicket(int id)
    {
        try
        {
            // Encontra o ticket no banco de dados
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                _logger.LogWarning($"Ticket {id} não encontrado.");
                return NotFound("Ticket não encontrado.");
            }

            // Verifica se o ticket ainda está em espera
            if (ticket.Status != TicketStatus.EmEspera)
            {
                return BadRequest("A senha não está mais em espera.");
            }

            // Atualiza o status para 'Chamado'
            ticket.Status = TicketStatus.Chamado;
            ticket.CalledAt = DateTime.Now;

            // Salva as mudanças no banco
            await _context.SaveChangesAsync();

            var guiche = ticket.Guiche?.Nome ?? "Não atribuído";

            // Envia a senha atualizada com o status e guichê para todos os clientes conectados via SignalR
            await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", ticket.Number, guiche, ticket.Status);

            return Ok(new { ticket = ticket.Number, status = ticket.Status, guiche = guiche });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao chamar o ticket {id}: {ex.Message}, Detalhes: {ex.StackTrace}");
            return StatusCode(500, "Erro interno ao processar o ticket.");
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult SelecionarTipo(string service)
    {
        ViewBag.Service = service;
        return View();
    }

    // Método para retornar arquivos de mídia (vídeos/imagens)
    [HttpGet]
    public IActionResult GetMediaFiles()
    {
        var mediaPath = Path.Combine(_webHostEnvironment.WebRootPath, "videos");

        if (Directory.Exists(mediaPath))
        {
            var files = Directory.GetFiles(mediaPath)
                .Select(f => f.Replace(_webHostEnvironment.WebRootPath, "").Replace("\\", "/"))
                .Where(f => new[] { ".mp4", ".jpg", ".jpeg", ".png", ".gif" }
                .Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            _logger.LogInformation($"Arquivos de mídia encontrados: {string.Join(", ", files)}");

            return Json(files);
        }
        else
        {
            return NotFound("Pasta de mídia não encontrada.");
        }
    }
}
