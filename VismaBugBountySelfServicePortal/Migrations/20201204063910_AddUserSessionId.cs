using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VismaBugBountySelfServicePortal.Migrations
{
    public partial class AddUserSessionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSession",
                table: "UserSession");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "UserSession",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSession",
                table: "UserSession",
                columns: new[] { "Email", "SessionId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSession",
                table: "UserSession");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "UserSession");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSession",
                table: "UserSession",
                column: "Email");
        }
    }
}
