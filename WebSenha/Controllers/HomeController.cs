using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WebSenha.Data;
using WebSenha.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebSenha.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QueueContext _context; // Adicione o contexto de dados

        public HomeController(ILogger<HomeController> logger, QueueContext context)
        {
            _logger = logger;
            _context = context; // Inicialize o contexto
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
        public async Task<IActionResult> ListaSenhas()
        {
            var senhas = await _context.Painels.ToListAsync(); // Obtenha a lista de senhas
            return View(senhas); // Passa a lista de senhas para a view
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}