using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class aff_payments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FullYearPayment",
                table: "AffiliatePayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextPlannedPaymentDate",
                table: "AffiliatePayments",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullYearPayment",
                table: "AffiliatePayments");

            migrationBuilder.DropColumn(
                name: "NextPlannedPaymentDate",
                table: "AffiliatePayments");
        }
    }
}
