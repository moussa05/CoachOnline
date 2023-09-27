using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class subs_for_library : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BooksNo",
                table: "LibraryAccounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CdsNo",
                table: "LibraryAccounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionUrl",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadersNo",
                table: "LibraryAccounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SIGBName",
                table: "LibraryAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideosNo",
                table: "LibraryAccounts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LibrarySubscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    LibraryId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SubscriptionEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PricingName = table.Column<string>(type: "text", nullable: true),
                    NumberOfActiveUsers = table.Column<int>(type: "integer", nullable: false),
                    TimePeriod = table.Column<byte>(type: "smallint", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    AccessType = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibrarySubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibrarySubscription_LibraryAccounts_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "LibraryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySubscription_LibraryId",
                table: "LibrarySubscription",
                column: "LibraryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibrarySubscription");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "users");

            migrationBuilder.DropColumn(
                name: "BooksNo",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "CdsNo",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "InstitutionUrl",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "ReadersNo",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "SIGBName",
                table: "LibraryAccounts");

            migrationBuilder.DropColumn(
                name: "VideosNo",
                table: "LibraryAccounts");
        }
    }
}
