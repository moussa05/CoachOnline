using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class start_for_lib_accounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LibraryName",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetNo",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LibraryAccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Token = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<long>(type: "bigint", nullable: false),
                    ValidTo = table.Column<long>(type: "bigint", nullable: false),
                    Disposed = table.Column<bool>(type: "boolean", nullable: false),
                    LibraryAccountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryAccessTokens_LibraryAccounts_LibraryAccountId",
                        column: x => x.LibraryAccountId,
                        principalTable: "LibraryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryReferents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Fname = table.Column<string>(type: "text", nullable: true),
                    Lname = table.Column<string>(type: "text", nullable: true),
                    PhoneNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "text", nullable: true),
                    LibraryAccountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryReferents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryReferents_LibraryAccounts_LibraryAccountId",
                        column: x => x.LibraryAccountId,
                        principalTable: "LibraryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryAccessTokens_LibraryAccountId",
                table: "LibraryAccessTokens",
                column: "LibraryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryReferents_LibraryAccountId",
                table: "LibraryReferents",
                column: "LibraryAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryAccessTokens");

            migrationBuilder.DropTable(
                name: "LibraryReferents");

            migrationBuilder.DropColumn(
                name: "City",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "LibraryName",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "StreetNo",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "LibraryAccounts");
        }
    }
}
