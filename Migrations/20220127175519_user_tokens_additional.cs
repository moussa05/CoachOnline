using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class user_tokens_additional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAllowedToWatch",
                table: "userLogins",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAllowedToWatch",
                table: "userLogins");
        }
    }
}
