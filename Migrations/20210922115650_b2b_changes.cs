using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class b2b_changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "AccessType",
                table: "B2BPricings",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "B2BAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "B2BAccountServices",
                columns: table => new
                {
                    B2BAccountId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BAccountServices", x => new { x.B2BAccountId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_B2BAccountServices_B2BAccounts_B2BAccountId",
                        column: x => x.B2BAccountId,
                        principalTable: "B2BAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_B2BAccountServices_B2BPricings_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "B2BPricings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2BAccountServices_ServiceId",
                table: "B2BAccountServices",
                column: "ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B2BAccountServices");

            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "B2BPricings");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "B2BAccounts");
        }
    }
}
