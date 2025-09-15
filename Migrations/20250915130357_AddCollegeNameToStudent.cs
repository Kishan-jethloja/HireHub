using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class AddCollegeNameToStudent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollegeName",
                table: "Students",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollegeName",
                table: "Students");
        }
    }
}
