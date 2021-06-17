using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Horth.Service.Email.Migrations
{
    public partial class stats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedExternal",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "FailedInternal",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "RetriesExternal",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "RetriesInternal",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "SentExternal",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "StatDay",
                table: "EmailStat");

            migrationBuilder.RenameColumn(
                name: "SentInternal",
                table: "EmailStat",
                newName: "Result");

            migrationBuilder.RenameColumn(
                name: "Client",
                table: "EmailStat",
                newName: "To");

            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "EmailStat",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "EmailStat",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "EmailStat",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "From",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "EmailStat");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "EmailStat");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "EmailStat",
                newName: "Client");

            migrationBuilder.RenameColumn(
                name: "Result",
                table: "EmailStat",
                newName: "SentInternal");

            migrationBuilder.AddColumn<int>(
                name: "FailedExternal",
                table: "EmailStat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailedInternal",
                table: "EmailStat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetriesExternal",
                table: "EmailStat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetriesInternal",
                table: "EmailStat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SentExternal",
                table: "EmailStat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatDay",
                table: "EmailStat",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
