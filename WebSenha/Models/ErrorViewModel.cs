using System;

namespace WebSenha.Models
{
    // Modelo que representa a informação de erro para a exibição
    public class ErrorViewModel
    {
        // ID da requisição que gerou o erro
        public string RequestId { get; set; }

        // Propriedade que indica se o RequestId deve ser mostrado
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
