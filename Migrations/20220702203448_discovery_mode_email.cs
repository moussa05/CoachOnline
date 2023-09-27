using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class discovery_mode_email : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndOdDiscoveryModeEmailSend",
                table: "users");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndOdDiscoveryModeEmailSendDate",
                table: "users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "EndOdDiscoveryModeStatus",
                table: "users",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndOdDiscoveryModeEmailSendDate",
                table: "users");

            migrationBuilder.DropColumn(
                name: "EndOdDiscoveryModeStatus",
                table: "users");

            migrationBuilder.AddColumn<bool>(
                name: "EndOdDiscoveryModeEmailSend",
                table: "users",
                type: "boolean",
                nullable: true);
        }
    }
}
