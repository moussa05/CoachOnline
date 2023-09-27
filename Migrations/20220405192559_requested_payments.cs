using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class requested_payments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestedPaymentId",
                table: "AffiliatePayments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RequestedPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PayPalEmail = table.Column<string>(type: "text", nullable: true),
                    PayPalPhone = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    PaymentValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusChangeDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestedPayments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AffiliatePayments_RequestedPaymentId",
                table: "AffiliatePayments",
                column: "RequestedPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AffiliatePayments_RequestedPayments_RequestedPaymentId",
                table: "AffiliatePayments",
                column: "RequestedPaymentId",
                principalTable: "RequestedPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AffiliatePayments_RequestedPayments_RequestedPaymentId",
                table: "AffiliatePayments");

            migrationBuilder.DropTable(
                name: "RequestedPayments");

            migrationBuilder.DropIndex(
                name: "IX_AffiliatePayments_RequestedPaymentId",
                table: "AffiliatePayments");

            migrationBuilder.DropColumn(
                name: "RequestedPaymentId",
                table: "AffiliatePayments");
        }
    }
}
