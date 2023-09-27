using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class aff_for_coaches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAffiliateACoach",
                table: "Affiliates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAffiliateCoach",
                table: "AffiliatePayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ForCoach",
                table: "AffiliateLinks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAffiliateACoach",
                table: "Affiliates");

            migrationBuilder.DropColumn(
                name: "IsAffiliateCoach",
                table: "AffiliatePayments");

            migrationBuilder.DropColumn(
                name: "ForCoach",
                table: "AffiliateLinks");
        }
    }
}
