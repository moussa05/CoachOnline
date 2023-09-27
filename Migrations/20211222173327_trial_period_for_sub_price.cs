using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class trial_period_for_sub_price : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrialDays",
                table: "SubscriptionPrice",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrialDays",
                table: "SubscriptionPrice");
        }
    }
}
