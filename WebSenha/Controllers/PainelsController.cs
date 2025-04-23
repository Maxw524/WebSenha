using Microsoft.AspNetCore.Mvc;
using WebSenha.Data;
using WebSenha.Models;
using System.Linq;
using System;
using WebSenha.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace WebSenha.Controllers
{
    public class PainelsController : Controller
    {
        private readonly QueueContext _contexto;
        private readonly ITokenService _tokenService;
        private readonly IHubContext<SenhaHub> _hubContext;

        public PainelsController(QueueContext contexto, ITokenService tokenService, IHubContext<SenhaHub> hubContext)
        {
            _contexto = contexto;
            _tokenService = tokenService;
            _hubContext = hubContext;
        }

        // Página de Atendimento
        public IActionResult Atendimento()
        {
            var senhas = _contexto.Painels
                .Where(p => p.Status == StatusSenha.Pendente)
                .OrderBy(p => p.CriadoEm)
                .ToList();

            ViewData["SenhasGeradas"] = senhas;
            return View();
        }

        // Endpoint para carregar senhas na fila, incluindo a contagem para modo automático
        [HttpGet("painels/GetSenhasFila")]
        public JsonResult GetSenhasFila(string tipoAtendimento, string tipoServico)
        {
            try
            {
                // Se o tipo de atendimento for automático, retornamos a contagem das senhas pendentes
                if (tipoAtendimento == "Automático")
                {
                    // Contar senhas pendentes para o tipo de serviço (Caixa ou Atendimento)
                    var countSenhasPendentes = _contexto.Painels
                        .Where(p => p.Status == StatusSenha.Pendente && p.Service == tipoServico)
                        .Count();

                    return Json(new { sucesso = true, tipo = "Automático", count = countSenhasPendentes });
                }
                else
                {
                    // Se o tipo de atendimento não for automático, retornamos a lista de senhas
                    var senhasFila = _contexto.Painels
                        .Where(p => p.Status == StatusSenha.Pendente && p.Service == tipoServico)
                        .OrderBy(p => p.CriadoEm)
                        .Select(p => new
                        {
                            p.Senha,
                            p.Tipo,
                            p.Status,
                            p.Service
                        })
                        .ToList();

                    return senhasFila.Any()
                        ? Json(new { sucesso = true, tipo = tipoAtendimento, senhas = senhasFila })
                        : Json(new { sucesso = false, mensagem = "Nenhuma senha pendente encontrada." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = $"Erro ao carregar senhas: {ex.Message}" });
            }
        }


        [HttpPost("painels/chamar-proxima-senha")]
        public async Task<JsonResult> ChamarProximaSenha(string tipoAtendimento, string guiche, string tipoServico)
        {
            // Validação de parâmetros
            if (string.IsNullOrEmpty(tipoAtendimento) || string.IsNullOrEmpty(guiche) || string.IsNullOrEmpty(tipoServico))
            {
                return Json(new { erro = "Tipo de atendimento, guichê e tipo de serviço são obrigatórios." });
            }

            // Verificar se os valores de tipoAtendimento e tipoServico são válidos
            var tiposServicoValidos = new[] { "Caixa", "Atendimento" }; // Exemplos de tipos de serviço válidos
            if (!tiposServicoValidos.Contains(tipoServico))
            {
                return Json(new { erro = "Tipo de serviço inválido." });
            }

            var tiposAtendimentoValidos = new[] { "P", "N", "A" }; // Exemplos de tipos de atendimento válidos
            if (!tiposAtendimentoValidos.Contains(tipoAtendimento))
            {
                return Json(new { erro = "Tipo de atendimento inválido." });
            }

            try
            {
                // Iniciar uma transação para garantir integridade de dados e evitar concorrência
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Buscar a próxima senha pendente para o tipo de atendimento e tipo de serviço
                    var senhaProxima = await _contexto.Painels
                        .Where(p => p.Status == StatusSenha.Pendente
                                   && p.Service == tipoServico
                                   && (p.Tipo == "P" || p.Tipo == "N" || p.Tipo == tipoAtendimento))
                        .OrderBy(p => p.CriadoEm)
                        .FirstOrDefaultAsync();

                    // Se não encontrar uma senha, retornar erro
                    if (senhaProxima == null)
                    {
                        return Json(new { erro = "Nenhuma senha pendente para este tipo de atendimento e serviço." });
                    }

                    // Verificar se o status da senha ainda está Pendente
                    if (senhaProxima.Status != StatusSenha.Pendente)
                    {
                        return Json(new { erro = "A senha já foi alterada ou chamada por outro guichê." });
                    }

                    // Alterar o status da senha para Chamado e associar ao guichê
                    senhaProxima.Status = StatusSenha.Chamado;
                    senhaProxima.Guiche = guiche;

                    // Salvar as alterações no banco de dados
                    await _contexto.SaveChangesAsync();

                    // Enviar a atualização para todos os clientes conectados via SignalR
                    await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", senhaProxima.Senha, senhaProxima.Tipo, guiche, senhaProxima.Status.ToString());

                    // Completar a transação, garantindo que todas as operações sejam persistidas
                    scope.Complete();
                


                // Retornar sucesso e a senha que foi chamada
                return Json(new { sucesso = true, ticket = senhaProxima.Senha });
                }
            }
            catch (Exception ex)
            {
                // Caso ocorra um erro, retornar a mensagem de erro com detalhes
                return Json(new { erro = "Erro ao chamar a próxima senha", detalhes = ex.Message });
            }
        }

        // Metodo para Finalizar atendimento 

        [HttpPost("painels/finalizar-atendimento")]
        public async Task<JsonResult> FinalizarAtendimento([FromBody] FinalizarAtendimentoRequest request)
        {
            if (string.IsNullOrEmpty(request.Senha))
                return Json(new { erro = "Senha é obrigatória." });

            try
            {
                var senhaFinalizada = await _contexto.Painels
                    .FirstOrDefaultAsync(p => p.Senha == request.Senha);

                if (senhaFinalizada == null)
                    return Json(new { erro = "Senha não encontrada." });

                // Atribui o status "Finalizado" (Status 3)
                senhaFinalizada.Status = StatusSenha.Finalizado;
                await _contexto.SaveChangesAsync();

                // Notificar os clientes sobre a atualização do status
                await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", senhaFinalizada.Senha, senhaFinalizada.Tipo, senhaFinalizada.Guiche, "Finalizado");

                return Json(new { sucesso = true, mensagem = $"Senha {request.Senha} marcada como Finalizada." });
            }
            catch (Exception ex)
            {
                return Json(new { erro = $"Erro ao finalizar atendimento: {ex.Message}" });
            }
        }


        // Encaminhar Senha
        [HttpPost("painels/encaminhar-senha")]
        public async Task<IActionResult> EncaminharSenha([FromBody] EncaminharSenhaRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Senha) || string.IsNullOrEmpty(request.DestinoServico))
                return Json(new { sucesso = false, erro = "Senha e destino do serviço são obrigatórios." });

            var painel = await _contexto.Painels.FirstOrDefaultAsync(p => p.Senha == request.Senha);
            if (painel == null)
                return Json(new { sucesso = false, erro = "Senha não encontrada." });

            painel.Service = request.DestinoServico;
            painel.Status = StatusSenha.Encaminhada;

            await _contexto.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", painel.Senha, painel.Tipo, painel.Service, painel.Status.ToString());

            return Json(new { sucesso = true, mensagem = $"Senha {request.Senha} encaminhada para {request.DestinoServico}." });
        }


        // Obter senhas encaminhadas
        [HttpGet("painels/GetSenhasEncaminhadas")]
        public JsonResult GetSenhasEncaminhadas()
        {
            try
            {
                // Buscar as senhas com status "Encaminhada"
                var senhasEncaminhadas = _contexto.Painels
                    .Where(p => p.Status == StatusSenha.Encaminhada)
                    .OrderBy(p => p.CriadoEm)
                    .Select(p => new
                    {
                        p.Senha,
                        p.Tipo,
                        p.Service,
                        p.Status,
                        p.Guiche
                    })
                    .ToList();

                // Verificar se há senhas encaminhadas e retornar o resultado
                return senhasEncaminhadas.Any()
                    ? Json(new { sucesso = true, senhas = senhasEncaminhadas })
                    : Json(new { sucesso = false, mensagem = "Nenhuma senha encaminhada encontrada." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, erro = $"Erro ao carregar senhas encaminhadas: {ex.Message}" });
            }
        }

        // Chamar senha encaminhada novamente
        [HttpPost("painels/chamar-novamente-encaminhada")]
        public async Task<JsonResult> ChamarNovamenteEncaminhada([FromBody] ChamarNovamenteRequest request)
        {
            if (string.IsNullOrEmpty(request.Senha) || string.IsNullOrEmpty(request.Guiche))
                return Json(new { erro = "Senha, tipo de atendimento e guichê são obrigatórios." });

            try
            {
                // Procurar pela senha encaminhada
                var senhaEncaminhada = await _contexto.Painels
                    .FirstOrDefaultAsync(p => p.Senha == request.Senha && p.Status == StatusSenha.Encaminhada);

                if (senhaEncaminhada == null)
                    return Json(new { erro = "Senha não encontrada ou não está encaminhada." });

                // Alterar o status para "Chamado" e garantir que o guichê seja corretamente atribuído
                senhaEncaminhada.Status = StatusSenha.Chamado;
                senhaEncaminhada.Guiche = request.Guiche;

                await _contexto.SaveChangesAsync();

                // Notificar todos os clientes da alteração
                await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", senhaEncaminhada.Senha, senhaEncaminhada.Tipo, senhaEncaminhada.Guiche, senhaEncaminhada.Status.ToString());

                return Json(new { sucesso = true, mensagem = $"Senha {request.Senha} foi chamada novamente e está na fila." });
            }
            catch (Exception ex)
            {
                return Json(new { erro = $"Erro ao chamar novamente a senha: {ex.Message}" });
            }
        }


        // Chamar senha novamente
        [HttpPost("painels/chamar-novamente")]
        public async Task<JsonResult> ChamarNovamente([FromBody] ChamarNovamenteRequest request)
        {
            if (string.IsNullOrEmpty(request.Senha) || string.IsNullOrEmpty(request.Guiche)) // Verificar se a Senha e Guichê foram fornecidos
                return Json(new { erro = "Senha e Guichê são obrigatórios." });

            try
            {
                // Procurar pela senha no banco de dados
                var senhaSelecionada = await _contexto.Painels
                    .FirstOrDefaultAsync(p => p.Senha == request.Senha);

                if (senhaSelecionada == null)
                    return Json(new { erro = "Senha não encontrada." });

                // Alterar o status para "ChamarNovamente" (sem alterar o guichê)
                senhaSelecionada.Status = StatusSenha.Chamado;

                // Atualizar o guichê com o valor recebido do frontend
                senhaSelecionada.Guiche = request.Guiche;

                // Salvar a alteração do status e guichê
                await _contexto.SaveChangesAsync();

                // Notificar os clientes que a senha foi chamada novamente e associada ao guichê
                await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada",
                    senhaSelecionada.Senha, senhaSelecionada.Tipo, senhaSelecionada.Guiche, "Chamado");

                return Json(new { sucesso = true, mensagem = $"Senha {request.Senha} foi chamada novamente no guichê {request.Guiche}." });
            }
            catch (Exception ex)
            {
                return Json(new { erro = $"Erro ao chamar novamente a senha: {ex.Message}" });
            }
        }


        //Altera o status para Não comapreceu 
        [HttpPost("painels/nao-compareceu")]
        public async Task<JsonResult> NaoCompareceu([FromBody] NaoCompareceuRequest request)
        {
            if (string.IsNullOrEmpty(request.Senha))
                return Json(new { erro = "Senha é obrigatória." });

            try
            {
                var senhaNaoCompareceu = await _contexto.Painels
                    .FirstOrDefaultAsync(p => p.Senha == request.Senha);

                if (senhaNaoCompareceu == null)
                    return Json(new { erro = "Senha não encontrada." });

                // Atribui o status "NaoCompareceu" (Status 6)
                senhaNaoCompareceu.Status = StatusSenha.NaoCompareceu;
                await _contexto.SaveChangesAsync();

                // Notificar os clientes sobre a atualização do status
                await _hubContext.Clients.All.SendAsync("ReceberSenhaAtualizada", senhaNaoCompareceu.Senha, senhaNaoCompareceu.Tipo, senhaNaoCompareceu.Guiche, "NaoCompareceu");

                return Json(new { sucesso = true, mensagem = $"Senha {request.Senha} marcada como Não Compareceu." });
            }
            catch (Exception ex)
            {
                return Json(new { erro = $"Erro ao marcar como Não Compareceu: {ex.Message}" });
            }
        }
    }
}
