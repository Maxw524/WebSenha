using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSenha.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Painel_TipoSenha_TipoId",
                table: "Painel");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Painel_PainelId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Painel_TipoId",
                table: "Painel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TipoId",
                table: "Painel");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Tickets");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "Ticket");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Ticket",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Ticket",
                newName: "Numero");

            migrationBuilder.RenameColumn(
                name: "IssuedAt",
                table: "Ticket",
                newName: "EmitidoEm");

            migrationBuilder.RenameColumn(
                name: "CalledAt",
                table: "Ticket",
                newName: "ChamadoEm");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_PainelId",
                table: "Ticket",
                newName: "IX_Ticket_PainelId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Painel",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Painel",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Numero",
                table: "Ticket",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Painel_PainelId",
                table: "Ticket",
                column: "PainelId",
                principalTable: "Painel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Painel_PainelId",
                table: "Ticket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Painel");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Painel");

            migrationBuilder.RenameTable(
                name: "Ticket",
                newName: "Tickets");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tickets",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "Tickets",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "EmitidoEm",
                table: "Tickets",
                newName: "IssuedAt");

            migrationBuilder.RenameColumn(
                name: "ChamadoEm",
                table: "Tickets",
                newName: "CalledAt");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_PainelId",
                table: "Tickets",
                newName: "IX_Tickets_PainelId");

            migrationBuilder.AddColumn<int>(
                name: "TipoId",
                table: "Painel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tickets",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Tickets",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Painel_TipoId",
                table: "Painel",
                column: "TipoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Painel_TipoSenha_TipoId",
                table: "Painel",
                column: "TipoId",
                principalTable: "TipoSenha",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Painel_PainelId",
                table: "Tickets",
                column: "PainelId",
                principalTable: "Painel",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
