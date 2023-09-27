using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class coupon_valid_date : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CouponValidDate",
                table: "users",
                type: "timestamp without time zone",
                nullable: true);

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
                    Name = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    PercentOff = table.Column<decimal>(type: "numeric", nullable: true),
                    AmountOff = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    DurationInMonths = table.Column<int>(type: "integer", nullable: true),
                    AvailableForInfluencers = table.Column<bool>(type: "boolean", nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "CouponValidDate",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "BillingPlans");
        }
    }
}
