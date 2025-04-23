using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
namespace WebSenha.Models
{
    public enum StatusSenha
    {
        Pendente = 0,   
        Chamado = 1,
        Concluido = 2,
        Finalizado= 3,
        Encaminhada= 4,
        ChamarNovamente= 5,
        NaoCompareceu = 6,
       
    }
}
