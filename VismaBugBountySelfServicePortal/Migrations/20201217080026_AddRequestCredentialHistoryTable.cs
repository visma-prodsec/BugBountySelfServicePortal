using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VismaBugBountySelfServicePortal.Migrations
{
    public partial class AddRequestCredentialHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestCredentialHistory",
                columns: table => new
                {
                    SetId = table.Column<string>(nullable: false),
                    AssetName = table.Column<string>(nullable: false),
                    HackerName = table.Column<string>(nullable: false),
                    RequestDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestCredentialHistory", x => new { x.SetId, x.AssetName, x.HackerName });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestCredentialHistory");
        }
    }
}
