using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class library_subs_changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NegotiatedPrice",
                table: "LibrarySubscription",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PricePlanId",
                table: "LibrarySubscription",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySubscription_PricePlanId",
                table: "LibrarySubscription",
                column: "PricePlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_LibrarySubscription_B2BPricings_PricePlanId",
                table: "LibrarySubscription",
                column: "PricePlanId",
                principalTable: "B2BPricings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibrarySubscription_B2BPricings_PricePlanId",
                table: "LibrarySubscription");

            migrationBuilder.DropIndex(
                name: "IX_LibrarySubscription_PricePlanId",
                table: "LibrarySubscription");

            migrationBuilder.DropColumn(
                name: "NegotiatedPrice",
                table: "LibrarySubscription");

            migrationBuilder.DropColumn(
                name: "PricePlanId",
                table: "LibrarySubscription");
        }
    }
}
