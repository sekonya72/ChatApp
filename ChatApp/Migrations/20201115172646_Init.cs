using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHAT_Groups",
                columns: table => new
                {
                    GroupId = table.Column<string>(nullable: false),
                    When = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "CHAT_Users",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    ConnectionID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CHAT_Messages",
                columns: table => new
                {
                    MessageId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    When = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_CHAT_Messages_CHAT_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "CHAT_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CHAT_UserGroups",
                columns: table => new
                {
                    UserID = table.Column<string>(nullable: false),
                    GroupID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_UserGroups", x => new { x.UserID, x.GroupID });
                    table.ForeignKey(
                        name: "FK_CHAT_UserGroups_CHAT_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "CHAT_Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CHAT_UserGroups_CHAT_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "CHAT_Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_Messages_UserId",
                table: "CHAT_Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_UserGroups_GroupID",
                table: "CHAT_UserGroups",
                column: "GroupID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHAT_Messages");

            migrationBuilder.DropTable(
                name: "CHAT_UserGroups");

            migrationBuilder.DropTable(
                name: "CHAT_Groups");

            migrationBuilder.DropTable(
                name: "CHAT_Users");
        }
    }
}
