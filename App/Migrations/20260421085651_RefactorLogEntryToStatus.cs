using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLogEntryToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "ColumnName",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "PreviousValue",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "ValueType",
                table: "LogEntries");

            migrationBuilder.AddColumn<int>(
                name: "NewStatus",
                table: "LogEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "LogEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "LogEntries");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "LogEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ColumnName",
                table: "LogEntries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "LogEntries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreviousValue",
                table: "LogEntries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                table: "LogEntries",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
