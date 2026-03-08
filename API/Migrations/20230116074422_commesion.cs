using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class commesion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarReportCommission",
                table: "AuctionRecord");

            migrationBuilder.DropColumn(
                name: "Insurance",
                table: "AuctionRecord");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CarReportCommission",
                table: "AuctionRecord",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Insurance",
                table: "AuctionRecord",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
