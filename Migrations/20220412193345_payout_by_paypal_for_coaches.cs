using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class payout_by_paypal_for_coaches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "PaymentType",
                table: "RequestedPayments",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "RequestedPaymentId",
                table: "CoachDailyBalance",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachDailyBalance_RequestedPaymentId",
                table: "CoachDailyBalance",
                column: "RequestedPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachDailyBalance_RequestedPayments_RequestedPaymentId",
                table: "CoachDailyBalance",
                column: "RequestedPaymentId",
                principalTable: "RequestedPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachDailyBalance_RequestedPayments_RequestedPaymentId",
                table: "CoachDailyBalance");

            migrationBuilder.DropIndex(
                name: "IX_CoachDailyBalance_RequestedPaymentId",
                table: "CoachDailyBalance");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "RequestedPayments");

            migrationBuilder.DropColumn(
                name: "RequestedPaymentId",
                table: "CoachDailyBalance");
        }
    }
}
