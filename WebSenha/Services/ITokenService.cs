namespace WebSenha.Services
{
    public interface ITokenService
    {
        SenhaGerada GerarNovaSenha(int tipoId, string service);
    }

    public class SenhaGerada
    {
        public string Senha { get; set; }
        public string Tipo { get; set; }
    }
}