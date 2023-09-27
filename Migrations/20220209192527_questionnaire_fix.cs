using Microsoft.EntityFrameworkCore.Migrations;

namespace CoachOnline.Migrations
{
    public partial class questionnaire_fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireOptions_Questionnaires_QuestionnaireId",
                table: "QuestionnaireOptions");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionnaireId",
                table: "QuestionnaireOptions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireOptions_Questionnaires_QuestionnaireId",
                table: "QuestionnaireOptions",
                column: "QuestionnaireId",
                principalTable: "Questionnaires",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireOptions_Questionnaires_QuestionnaireId",
                table: "QuestionnaireOptions");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionnaireId",
                table: "QuestionnaireOptions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireOptions_Questionnaires_QuestionnaireId",
                table: "QuestionnaireOptions",
                column: "QuestionnaireId",
                principalTable: "Questionnaires",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
