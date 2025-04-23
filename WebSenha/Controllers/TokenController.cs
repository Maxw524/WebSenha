using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebSenha.Data;
using WebSenha.Models;
using WebSenha.Services;
using System;
using System.Linq;

public class TokenController : Controller
{
    private readonly QueueContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenController> _logger;

    public TokenController(QueueContext context, ITokenService tokenService, ILogger<TokenController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult GerarToken(int TipoId, string Service)
    {
        if (string.IsNullOrEmpty(Service))
        {
            return Json(new { erro = "Serviço não especificado" });
        }

        var resultado = _tokenService.GerarNovaSenha(TipoId, Service);

        if (resultado == null)
        {
            return Json(new { erro = "Erro ao gerar a senha. Tente novamente mais tarde." });
        }

        // Modificando a lógica da senha para inserir "P" ou "N" como terceira letra
        string tipoSufixo = (resultado.Tipo == "P") ? "P" : "N";
        string senhaGerada = resultado.Senha.Substring(0, 2) + tipoSufixo + resultado.Senha.Substring(2);

        // Salva a senha no banco
        var painel = new Painel
        {
            Senha = senhaGerada,
            Tipo = resultado.Tipo,
            Service = Service,
            Status = StatusSenha.Pendente,
            CriadoEm = DateTime.Now
        };

        try
        {
            _context.Painels.Add(painel);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao salvar a senha no banco: {ex.Message}");
            return Json(new { erro = "Erro ao salvar a senha. Tente novamente mais tarde." });
        }

        return Json(new { senha = painel.Senha, tipo = painel.Tipo });
    }

    [HttpPost]
    public IActionResult ChamarSenha(string senha)
    {
        var painel = _context.Painels.FirstOrDefault(p => p.Senha == senha && p.Status == StatusSenha.Pendente);

        if (painel != null)
        {
            try
            {
                painel.Status = StatusSenha.Chamado;
                _context.SaveChanges();
                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao atualizar o status da senha {senha}: {ex.Message}");
                return Json(new { erro = "Erro ao chamar a senha. Tente novamente mais tarde." });
            }
        }

        return Json(new { erro = "Senha não encontrada ou já foi chamada." });
    }

    [HttpGet]
    public IActionResult ListaSenhasPendentes()
    {
        var senhasPendentes = _context.Painels
                                       .Where(p => p.Status == StatusSenha.Pendente)
                                       .Select(p => new { p.Senha, p.Tipo })
                                       .ToList();

        return Json(new { senhas = senhasPendentes });
    }
}
