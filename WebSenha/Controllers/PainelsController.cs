using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WebSenha.Data; // Altere para o namespace correto do seu contexto
using WebSenha.Models;

namespace WebSenha.Controllers
{
    public class PainelsController : Controller
    {
        private readonly QueueContext _contexto; // Altere para QueueContext

        public PainelsController(QueueContext contexto) // Altere para QueueContext
        {
            _contexto = contexto;
        }

        [HttpPost("/api/ListaSenhas")]
        public JsonResult ListaSenhas()
        {
            var senhas = _contexto.Painels.Take(3)
                .OrderByDescending(c => c.Id).ToList();

            // Verifica se existem senhas
            var senhaAtual = senhas.FirstOrDefault();
            return Json(new { senhas = senhas, senha = senhaAtual });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Painel painel)
        {
            if (ModelState.IsValid) // Verifica se o modelo é válido
            {
                try
                {
                    _contexto.Painels.Add(painel); // Altere para Painels
                    _contexto.SaveChanges();
                    return RedirectToAction(nameof(Create));
                }
                catch (Exception ex)
                {
                    // Registre o erro e retorne uma mensagem amigável
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar o painel: " + ex.Message);
                }
            }
            return View(painel); // Retorna a view com o modelo para correção de erros
        }
    }
}
