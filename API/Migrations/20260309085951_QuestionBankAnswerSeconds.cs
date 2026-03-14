using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class QuestionBankAnswerSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerSeconds",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.Sql("""
                UPDATE Questions
                SET AnswerSeconds = 30
                WHERE AnswerSeconds <= 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerSeconds",
                table: "Questions");
        }
    }
}
