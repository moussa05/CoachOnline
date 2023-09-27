using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class acess_tokens_for_b2b : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Comission",
                table: "B2BAccounts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComissionCurrency",
                table: "B2BAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractSignDate",
                table: "B2BAccounts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContractSigned",
                table: "B2BAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "B2BAcessToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<long>(type: "bigint", nullable: false),
                    ValidTo = table.Column<long>(type: "bigint", nullable: false),
                    Disposed = table.Column<bool>(type: "boolean", nullable: false),
                    B2BAccountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BAcessToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2BAcessToken_B2BAccounts_B2BAccountId",
                        column: x => x.B2BAccountId,
                        principalTable: "B2BAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2BAcessToken_B2BAccountId",
                table: "B2BAcessToken",
                column: "B2BAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B2BAcessToken");

            migrationBuilder.DropColumn(
                name: "Comission",
                table: "B2BAccounts");

            migrationBuilder.DropColumn(
                name: "ComissionCurrency",
                table: "B2BAccounts");

            migrationBuilder.DropColumn(
                name: "ContractSignDate",
                table: "B2BAccounts");

            migrationBuilder.DropColumn(
                name: "ContractSigned",
                table: "B2BAccounts");
        }
    }
}
