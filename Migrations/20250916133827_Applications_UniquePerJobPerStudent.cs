using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class Applications_UniquePerJobPerStudent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_JobPostingId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobPostingId_StudentUserId",
                table: "Applications",
                columns: new[] { "JobPostingId", "StudentUserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_JobPostingId_StudentUserId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobPostingId",
                table: "Applications",
                column: "JobPostingId");
        }
    }
}
