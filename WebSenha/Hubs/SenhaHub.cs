using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class SenhaHub : Hub
{
    // Método para notificar os clientes sobre a nova senha chamada
    public async Task AtualizarSenha(string senha, string tipo, int guiche, int status)
    {
        // Envia para todos os clientes conectados com todos os parâmetros
        await Clients.All.SendAsync("ReceberSenhaAtualizada", senha, tipo, guiche, status);
    }
}
