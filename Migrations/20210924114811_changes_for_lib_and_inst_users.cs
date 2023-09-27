using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class changes_for_lib_and_inst_users : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySubscription_B2BPricings_PricePlanId",
                table: "LibrarySubscription");

            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySubscription_LibraryAccounts_LibraryId",
                table: "LibrarySubscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LibrarySubscription",
                table: "LibrarySubscription");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LibrarySubscription");

            migrationBuilder.RenameTable(
                name: "LibrarySubscription",
                newName: "LibrarySubscriptions");

            migrationBuilder.RenameIndex(
                name: "IX_LibrarySubscription_PricePlanId",
                table: "LibrarySubscriptions",
                newName: "IX_LibrarySubscriptions_PricePlanId");

            migrationBuilder.RenameIndex(
                name: "IX_LibrarySubscription_LibraryId",
                table: "LibrarySubscriptions",
                newName: "IX_LibrarySubscriptions_LibraryId");

            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfessionId",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "LibrarySubscriptions",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LibrarySubscriptions",
                table: "LibrarySubscriptions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Professions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professions", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySubscriptions_B2BPricings_PricePlanId",
                table: "LibrarySubscriptions",
                column: "PricePlanId",
                principalTable: "B2BPricings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySubscriptions_LibraryAccounts_LibraryId",
                table: "LibrarySubscriptions",
                column: "LibraryId",
                principalTable: "LibraryAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySubscriptions_B2BPricings_PricePlanId",
                table: "LibrarySubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySubscriptions_LibraryAccounts_LibraryId",
                table: "LibrarySubscriptions");

            migrationBuilder.DropTable(
                name: "Professions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LibrarySubscriptions",
                table: "LibrarySubscriptions");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProfessionId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LibrarySubscriptions");

            migrationBuilder.RenameTable(
                name: "LibrarySubscriptions",
                newName: "LibrarySubscription");

            migrationBuilder.RenameIndex(
                name: "IX_LibrarySubscriptions_PricePlanId",
                table: "LibrarySubscription",
                newName: "IX_LibrarySubscription_PricePlanId");

            migrationBuilder.RenameIndex(
                name: "IX_LibrarySubscriptions_LibraryId",
                table: "LibrarySubscription",
                newName: "IX_LibrarySubscription_LibraryId");

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LibrarySubscription",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LibrarySubscription",
                table: "LibrarySubscription",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySubscription_B2BPricings_PricePlanId",
                table: "LibrarySubscription",
                column: "PricePlanId",
                principalTable: "B2BPricings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySubscription_LibraryAccounts_LibraryId",
                table: "LibrarySubscription",
                column: "LibraryId",
                principalTable: "LibraryAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
