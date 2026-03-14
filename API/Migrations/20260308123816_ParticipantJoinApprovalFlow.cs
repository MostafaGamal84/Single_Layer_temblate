using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class ParticipantJoinApprovalFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "GameParticipants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecisionByHostId",
                table: "GameParticipants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNote",
                table: "GameParticipants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JoinStatus",
                table: "GameParticipants",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "GameParticipants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "GameParticipants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "GameParticipants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("""
                UPDATE GameParticipants
                SET JoinStatus = 2
                WHERE JoinStatus = 1;

                UPDATE GameParticipants
                SET RequestedAt = COALESCE(NULLIF(RequestedAt, '0001-01-01T00:00:00'), JoinedAt, SYSUTCDATETIME());

                UPDATE GameParticipants
                SET ApprovedAt = COALESCE(ApprovedAt, JoinedAt)
                WHERE JoinStatus = 2;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "GameParticipants");

            migrationBuilder.DropColumn(
                name: "DecisionByHostId",
                table: "GameParticipants");

            migrationBuilder.DropColumn(
                name: "DecisionNote",
                table: "GameParticipants");

            migrationBuilder.DropColumn(
                name: "JoinStatus",
                table: "GameParticipants");

            migrationBuilder.DropColumn(
                name: "LeftAt",
                table: "GameParticipants");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "GameParticipants");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "GameParticipants");
        }
    }
}
