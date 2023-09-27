using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class payments_for_affiliates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AffiliatePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AffiliateId = table.Column<int>(type: "integer", nullable: false),
                    HostId = table.Column<int>(type: "integer", nullable: false),
                    PaymentCreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PaymentCurrency = table.Column<string>(type: "text", nullable: true),
                    PaymentValue = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentForMonth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsFirstPayment = table.Column<bool>(type: "boolean", nullable: false),
                    Transferred = table.Column<bool>(type: "boolean", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffiliatePayments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffiliatePayments");
        }
    }
}
