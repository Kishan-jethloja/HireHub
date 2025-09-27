using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class AddJobPostingIdToFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobPostingId",
                table: "Feedbacks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_JobPostingId",
                table: "Feedbacks",
                column: "JobPostingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_JobPostings_JobPostingId",
                table: "Feedbacks",
                column: "JobPostingId",
                principalTable: "JobPostings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_JobPostings_JobPostingId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_JobPostingId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "JobPostingId",
                table: "Feedbacks");
        }
    }
}
