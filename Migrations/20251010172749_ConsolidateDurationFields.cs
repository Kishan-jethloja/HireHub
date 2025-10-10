using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class ConsolidateDurationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "DurationYears",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "GoogleFormUrl",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "JobPostings",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApplyByUtc",
                table: "JobPostings",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "JobPostings",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "JobPostings");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "JobPostings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApplyByUtc",
                table: "JobPostings",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "JobPostings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationYears",
                table: "JobPostings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleFormUrl",
                table: "JobPostings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
