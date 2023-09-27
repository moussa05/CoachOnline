using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class episode_attachments_as_entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EpisodeAttachment_Episodes_EpisodeId",
                table: "EpisodeAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EpisodeAttachment",
                table: "EpisodeAttachment");

            migrationBuilder.RenameTable(
                name: "EpisodeAttachment",
                newName: "EpisodeAttachments");

            migrationBuilder.RenameIndex(
                name: "IX_EpisodeAttachment_EpisodeId",
                table: "EpisodeAttachments",
                newName: "IX_EpisodeAttachments_EpisodeId");

            migrationBuilder.AlterColumn<int>(
                name: "EpisodeId",
                table: "EpisodeAttachments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EpisodeAttachments",
                table: "EpisodeAttachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EpisodeAttachments_Episodes_EpisodeId",
                table: "EpisodeAttachments",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EpisodeAttachments_Episodes_EpisodeId",
                table: "EpisodeAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EpisodeAttachments",
                table: "EpisodeAttachments");

            migrationBuilder.RenameTable(
                name: "EpisodeAttachments",
                newName: "EpisodeAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_EpisodeAttachments_EpisodeId",
                table: "EpisodeAttachment",
                newName: "IX_EpisodeAttachment_EpisodeId");

            migrationBuilder.AlterColumn<int>(
                name: "EpisodeId",
                table: "EpisodeAttachment",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EpisodeAttachment",
                table: "EpisodeAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EpisodeAttachment_Episodes_EpisodeId",
                table: "EpisodeAttachment",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
