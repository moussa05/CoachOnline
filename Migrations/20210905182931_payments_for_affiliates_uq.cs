using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class payments_for_affiliates_uq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AffiliatePayments_AffiliateId_HostId_PaymentForMonth",
                table: "AffiliatePayments",
                columns: new[] { "AffiliateId", "HostId", "PaymentForMonth" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AffiliatePayments_AffiliateId_HostId_PaymentForMonth",
                table: "AffiliatePayments");
        }
    }
}
