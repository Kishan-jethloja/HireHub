using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class AddedDurationInJobPostings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "JobPostings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationYears",
                table: "JobPostings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "DurationYears",
                table: "JobPostings");
        }
    }
}
