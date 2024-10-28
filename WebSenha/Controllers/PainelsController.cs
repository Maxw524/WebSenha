using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WebSenha.Config;
using WebSenha.Models;

namespace WebSenha.Controllers
{
    public class PainelsController : Controller
    {
        private readonly Contexto _contexto;

        public PainelsController(Contexto Contexto)
        {
            _contexto = Contexto;
        }
        [HttpPost("/api/ListaSenhas")]
    public JsonResult ListaSenhas()
        {

            var senhas = _contexto.Painel.Take(3)
                .OrderByDescending(c => c.Id).ToList();
            return Json(new { senhas = senhas, senha = senhas.FirstOrDefault() });
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create ( Painel painel)
        {
           try
            {
                _contexto.Painel.Add(painel);
                _contexto.SaveChanges();

                return RedirectToAction(nameof(Create));
            }

            catch (Exception)
            {

                throw;
            }
        }


    }
}
