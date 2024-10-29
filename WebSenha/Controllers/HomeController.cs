using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WebSenha.Models;

namespace WebSenha.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Aqui você pode adicionar lógica para carregar dados iniciais
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Ação para obter a lista de senhas
        public IActionResult ListaSenhas()
        {
            // Lógica para obter e passar a lista de senhas para a view
            return View(); // Retorna a view correspondente
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
