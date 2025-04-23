using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSenha.Migrations
{
    /// <inheritdoc />
    public partial class CriarRelacionamentoPainelTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoSenha",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    criado_em = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    atualizado_em = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoSenha", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Painel",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Senha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Guiche = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Painel", x => x.id);
                    table.ForeignKey(
                        name: "FK_Painel_TipoSenha_TipoId",
                        column: x => x.TipoId,
                        principalTable: "TipoSenha",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    PainelId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Painel_PainelId",
                        column: x => x.PainelId,
                        principalTable: "Painel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Painel_TipoId",
                table: "Painel",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PainelId",
                table: "Tickets",
                column: "PainelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Painel");

            migrationBuilder.DropTable(
                name: "TipoSenha");
        }
    }
}
