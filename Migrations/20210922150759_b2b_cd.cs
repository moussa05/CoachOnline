using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class b2b_cd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_B2BAcessToken_B2BAccounts_B2BAccountId",
                table: "B2BAcessToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_B2BAcessToken",
                table: "B2BAcessToken");

            migrationBuilder.RenameTable(
                name: "B2BAcessToken",
                newName: "B2BAccountTokens");

            migrationBuilder.RenameIndex(
                name: "IX_B2BAcessToken_B2BAccountId",
                table: "B2BAccountTokens",
                newName: "IX_B2BAccountTokens_B2BAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_B2BAccountTokens",
                table: "B2BAccountTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_B2BAccountTokens_B2BAccounts_B2BAccountId",
                table: "B2BAccountTokens",
                column: "B2BAccountId",
                principalTable: "B2BAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_B2BAccountTokens_B2BAccounts_B2BAccountId",
                table: "B2BAccountTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_B2BAccountTokens",
                table: "B2BAccountTokens");

            migrationBuilder.RenameTable(
                name: "B2BAccountTokens",
                newName: "B2BAcessToken");

            migrationBuilder.RenameIndex(
                name: "IX_B2BAccountTokens_B2BAccountId",
                table: "B2BAcessToken",
                newName: "IX_B2BAcessToken_B2BAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_B2BAcessToken",
                table: "B2BAcessToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_B2BAcessToken_B2BAccounts_B2BAccountId",
                table: "B2BAcessToken",
                column: "B2BAccountId",
                principalTable: "B2BAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
