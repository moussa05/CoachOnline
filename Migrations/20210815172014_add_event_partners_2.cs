using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class add_event_partners_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPartner_Events_EventId",
                table: "EventPartner");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPartner",
                table: "EventPartner");

            migrationBuilder.RenameTable(
                name: "EventPartner",
                newName: "EventPartners");

            migrationBuilder.RenameIndex(
                name: "IX_EventPartner_EventId",
                table: "EventPartners",
                newName: "IX_EventPartners_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPartners",
                table: "EventPartners",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPartners_Events_EventId",
                table: "EventPartners",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPartners_Events_EventId",
                table: "EventPartners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventPartners",
                table: "EventPartners");

            migrationBuilder.RenameTable(
                name: "EventPartners",
                newName: "EventPartner");

            migrationBuilder.RenameIndex(
                name: "IX_EventPartners_EventId",
                table: "EventPartner",
                newName: "IX_EventPartner_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventPartner",
                table: "EventPartner",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPartner_Events_EventId",
                table: "EventPartner",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
