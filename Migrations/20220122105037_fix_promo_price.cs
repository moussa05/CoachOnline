using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class fix_promo_price : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingPlans_CouponResponse_CouponId",
                table: "BillingPlans");

            migrationBuilder.DropTable(
                name: "CouponResponse");

            migrationBuilder.DropIndex(
                name: "IX_BillingPlans_CouponId",
                table: "BillingPlans");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "BillingPlans");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponId",
                table: "BillingPlans",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CouponResponse",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AmountOff = table.Column<decimal>(type: "numeric", nullable: true),
                    AvailableForInfluencers = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    DurationInMonths = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PercentOff = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponResponse", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingPlans_CouponId",
                table: "BillingPlans",
                column: "CouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingPlans_CouponResponse_CouponId",
                table: "BillingPlans",
                column: "CouponId",
                principalTable: "CouponResponse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
