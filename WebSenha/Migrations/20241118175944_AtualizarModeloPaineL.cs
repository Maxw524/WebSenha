using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSenha.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarModeloPaineL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuicheId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Ticket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Guiche",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guiche", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_GuicheId",
                table: "Ticket",
                column: "GuicheId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Guiche_GuicheId",
                table: "Ticket",
                column: "GuicheId",
                principalTable: "Guiche",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Guiche_GuicheId",
                table: "Ticket");

            migrationBuilder.DropTable(
                name: "Guiche");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_GuicheId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "GuicheId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Ticket");
        }
    }
}
