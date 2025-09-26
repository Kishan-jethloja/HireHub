using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class ApplicationStatusForApplications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('Applications', 'Status') IS NULL BEGIN ALTER TABLE [Applications] ADD [Status] int NOT NULL CONSTRAINT DF_Applications_Status DEFAULT 0; END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('Applications', 'Status') IS NOT NULL BEGIN ALTER TABLE [Applications] DROP COLUMN [Status]; END");
        }
    }
}
