using System.ComponentModel.DataAnnotations;

public class EncaminharSenhaRequest
{
    [Required]
    public string Senha { get; set; }  // Senha que está sendo encaminhada

    [Required]
    public string DestinoServico { get; set; }  // O destino (Atendimento ou Caixa)
}
