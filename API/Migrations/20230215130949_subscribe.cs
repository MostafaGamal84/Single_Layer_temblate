using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class subscribe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubscribeId",
                table: "Client",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Subscribes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name_en = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuctioningCount = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Client_SubscribeId",
                table: "Client",
                column: "SubscribeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Subscribes_SubscribeId",
                table: "Client",
                column: "SubscribeId",
                principalTable: "Subscribes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_Subscribes_SubscribeId",
                table: "Client");

            migrationBuilder.DropTable(
                name: "Subscribes");

            migrationBuilder.DropIndex(
                name: "IX_Client_SubscribeId",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "SubscribeId",
                table: "Client");
        }
    }
}
