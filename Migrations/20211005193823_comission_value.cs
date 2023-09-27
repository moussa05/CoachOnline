using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class comission_value : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Comission",
                table: "B2BAccountServices",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComissionCurrency",
                table: "B2BAccountServices",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comission",
                table: "B2BAccountServices");

            migrationBuilder.DropColumn(
                name: "ComissionCurrency",
                table: "B2BAccountServices");
        }
    }
}
