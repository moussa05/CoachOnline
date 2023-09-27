using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class affiliator_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "AffiliatorType",
                table: "users",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "AffiliateModelType",
                table: "Affiliates",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "AffiliateModelType",
                table: "AffiliatePayments",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffiliatorType",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AffiliateModelType",
                table: "Affiliates");

            migrationBuilder.DropColumn(
                name: "AffiliateModelType",
                table: "AffiliatePayments");
        }
    }
}
