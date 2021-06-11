using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Horth.Service.Email.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailStat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StatDay = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Client = table.Column<string>(type: "TEXT", nullable: true),
                    SentExternal = table.Column<int>(type: "INTEGER", nullable: false),
                    RetriesExternal = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedExternal = table.Column<int>(type: "INTEGER", nullable: false),
                    SentInternal = table.Column<int>(type: "INTEGER", nullable: false),
                    RetriesInternal = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedInternal = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailStat", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailStat");
        }
    }
}
