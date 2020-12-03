using Microsoft.EntityFrameworkCore.Migrations;

namespace VismaBugBountySelfServicePortal.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    AssetName = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsVisible = table.Column<bool>(nullable: false),
                    IsOnHackerOne = table.Column<bool>(nullable: false),
                    IsOnPublicProgram = table.Column<bool>(nullable: false),
                    Columns = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.AssetName);
                });

            migrationBuilder.CreateTable(
                name: "Credential",
                columns: table => new
                {
                    SetId = table.Column<string>(nullable: false),
                    AssetName = table.Column<string>(nullable: false),
                    HackerName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credential", x => new { x.SetId, x.AssetName });
                });

            migrationBuilder.CreateTable(
                name: "CredentialValue",
                columns: table => new
                {
                    SetId = table.Column<string>(nullable: false),
                    RowNumber = table.Column<int>(nullable: false),
                    AssetName = table.Column<string>(nullable: false),
                    ColumnName = table.Column<string>(nullable: false),
                    ColumnValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialValue", x => new { x.AssetName, x.SetId, x.RowNumber, x.ColumnName });
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Email = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Email);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asset");

            migrationBuilder.DropTable(
                name: "Credential");

            migrationBuilder.DropTable(
                name: "CredentialValue");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
