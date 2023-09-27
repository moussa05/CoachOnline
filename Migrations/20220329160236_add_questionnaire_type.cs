using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class add_questionnaire_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "QuestionnaireType",
                table: "Questionnaires",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionnaireType",
                table: "Questionnaires");
        }
    }
}
