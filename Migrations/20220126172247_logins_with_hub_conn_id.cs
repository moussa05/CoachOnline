using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class logins_with_hub_conn_id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userLogins_users_UserId",
                table: "userLogins");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "userLogins",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HubConnectionId",
                table: "userLogins",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_userLogins_users_UserId",
                table: "userLogins",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userLogins_users_UserId",
                table: "userLogins");

            migrationBuilder.DropColumn(
                name: "HubConnectionId",
                table: "userLogins");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "userLogins",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_userLogins_users_UserId",
                table: "userLogins",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
