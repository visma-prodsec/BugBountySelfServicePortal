using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VismaBugBountySelfServicePortal.Migrations
{
    public partial class AddTransferTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Transferred",
                table: "Credential",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransferCredentialHistory",
                columns: table => new
                {
                    FromEmail = table.Column<string>(nullable: false),
                    ToEmail = table.Column<string>(nullable: false),
                    TransferredDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferCredentialHistory", x => new { x.FromEmail, x.ToEmail });
                });

            migrationBuilder.CreateTable(
                name: "UserSessionHistory",
                columns: table => new
                {
                    Email = table.Column<string>(nullable: false),
                    SessionId = table.Column<Guid>(nullable: false),
                    LoginDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessionHistory", x => new { x.Email, x.SessionId });
                });
            migrationBuilder.Sql("UPDATE Credential SET Transferred = 0 WHERE HackerName IS NOT NULL AND HackerName != ''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferCredentialHistory");

            migrationBuilder.DropTable(
                name: "UserSessionHistory");

            migrationBuilder.DropColumn(
                name: "Transferred",
                table: "Credential");
        }
    }
}
