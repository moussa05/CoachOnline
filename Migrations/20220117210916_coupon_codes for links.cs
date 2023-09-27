using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class coupon_codesforlinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "AffiliateLinks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LimitedPageView",
                table: "AffiliateLinks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "AffiliateLinks");

            migrationBuilder.DropColumn(
                name: "LimitedPageView",
                table: "AffiliateLinks");
        }
    }
}
