using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class phone_and_email_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNo",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInfo",
                table: "twoFATokens",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNo",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AdditionalInfo",
                table: "twoFATokens");
        }
    }
}
