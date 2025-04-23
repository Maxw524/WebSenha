using System.Collections.Generic;
using WebSenha.Services;

public class TokenService : ITokenService
{
    // Dicionário de contadores por tipo de serviço
    private static readonly Dictionary<string, int> Contadores = new Dictionary<string, int>
    {
        { "CaixaPreferencial", 1 },
        { "CaixaNormal", 1 },
        { "AtendimentoPreferencial", 1 },
        { "AtendimentoNormal", 1 }
    };

    private static readonly object LockObject = new object();

    public SenhaGerada GerarNovaSenha(int tipoId, string service)
    {
        string prefixo = "";
        string tipo = "";
        string chaveContador = "";

        // Lógica para determinar o serviço e o contador correspondente
        if (service == "Caixa")
        {
            prefixo = "CX";
            tipo = tipoId == 1 ? "P" : "N";
            chaveContador = tipoId == 1 ? "CaixaPreferencial" : "CaixaNormal";
        }
        else if (service == "Atendimento")
        {
            prefixo = "AT";
            tipo = tipoId == 1 ? "P" : "N";
            chaveContador = tipoId == 1 ? "AtendimentoPreferencial" : "AtendimentoNormal";
        }
        else
        {
            return null;  // Serviço desconhecido
        }

        // Geração do número da senha com base no contador
        int numero;
        lock (LockObject)
        {
            numero = Contadores[chaveContador]++;
        }

        string senhaGerada = $"{prefixo}-{numero:D3}"; // Formatação de senha com 3 dígitos

        // Retorna os dados gerados
        return new SenhaGerada
        {
            Senha = senhaGerada,
            Tipo = tipo
        };
    }
}