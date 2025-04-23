using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSenha.Models
{
    [Table("TipoSenha")] // Define o nome da tabela no banco de dados
    public class TipoSenha
    {
        [Key] // Marca como chave primária
        public int Id { get; set; }

        [Required] // Garantir que a descrição seja fornecida
        [StringLength(100)] // Limite de caracteres
        public string Descricao { get; set; }

        [Column("CriadoEm")] // Nome da coluna no banco de dados
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)] // Garante que o valor não será alterado
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow; // Data de criação, atribuindo o valor automaticamente

        [Column("AtualizadoEm")] // Nome da coluna no banco de dados
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow; // Data de atualização

        // Método para atualizar a data de modificação
        public void Atualizar()
        {
            AtualizadoEm = DateTime.UtcNow; // Atualiza a data de atualização
        }
    }
}