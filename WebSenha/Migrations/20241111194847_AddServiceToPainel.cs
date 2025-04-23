using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSenha.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceToPainel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Service",
                table: "Painel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Service",
                table: "Painel");
        }
    }
}
