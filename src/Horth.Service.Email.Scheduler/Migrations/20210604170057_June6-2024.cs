using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Horth.Service.Email.Scheduler.Migrations
{
    public partial class June62024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageQueueMessage");

            migrationBuilder.DropTable(
                name: "MessageQueueResult");

            migrationBuilder.CreateTable(
                name: "SchedulerResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Job = table.Column<string>(type: "TEXT", nullable: true),
                    ResultJoson = table.Column<string>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRun = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastRunEnd = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerResult", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerResult");

            migrationBuilder.CreateTable(
                name: "MessageQueueMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    From = table.Column<string>(type: "TEXT", nullable: true),
                    LastTry = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Service = table.Column<int>(type: "INTEGER", nullable: false),
                    serviceName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageQueueMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageQueueResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Job = table.Column<string>(type: "TEXT", nullable: true),
                    LastRun = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastRunEnd = table.Column<int>(type: "INTEGER", nullable: false),
                    ResultJoson = table.Column<string>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageQueueResult", x => x.Id);
                });
        }
    }
}
