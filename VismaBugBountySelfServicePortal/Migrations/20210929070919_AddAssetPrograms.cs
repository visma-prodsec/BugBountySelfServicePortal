using Microsoft.EntityFrameworkCore.Migrations;

namespace VismaBugBountySelfServicePortal.Migrations
{
    public partial class AddAssetPrograms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Programs",
                table: "Asset",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Programs",
                table: "Asset");
        }
    }
}
