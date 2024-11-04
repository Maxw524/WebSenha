using System;

namespace WebSenha.Models
{
    // Modelo que representa a informa��o de erro para a exibi��o
    public class ErrorViewModel
    {
        // ID da requisi��o que gerou o erro
        public string RequestId { get; set; }

        // Propriedade que indica se o RequestId deve ser mostrado
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}