using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class SessionFlowAndQuestionTimers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerSeconds",
                table: "QuizQuestions",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentQuestionEndsAt",
                table: "GameSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentQuestionRemainingSeconds",
                table: "GameSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentQuestionStartedAt",
                table: "GameSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionFlowMode",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE QuizQuestions
                SET AnswerSeconds = 30
                WHERE AnswerSeconds <= 0;

                UPDATE GameSessions
                SET QuestionFlowMode = 1
                WHERE QuestionFlowMode <= 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerSeconds",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionEndsAt",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionRemainingSeconds",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionStartedAt",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "QuestionFlowMode",
                table: "GameSessions");
        }
    }
}
