using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class AddApplicationIdToFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationId",
                table: "Feedbacks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ApplicationId",
                table: "Feedbacks",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Applications_ApplicationId",
                table: "Feedbacks",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Applications_ApplicationId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ApplicationId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Feedbacks");
        }
    }
}
