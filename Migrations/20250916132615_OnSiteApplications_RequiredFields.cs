using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class OnSiteApplications_RequiredFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobPostingId = table.Column<int>(nullable: false),
                    StudentUserId = table.Column<string>(nullable: false),
                    ApplicantName = table.Column<string>(maxLength: 200, nullable: false),
                    ApplicantEmail = table.Column<string>(maxLength: 200, nullable: false),
                    CollegeId = table.Column<string>(maxLength: 50, nullable: false),
                    LinkedInUrl = table.Column<string>(maxLength: 300, nullable: false),
                    GithubUrl = table.Column<string>(maxLength: 300, nullable: false),
                    Gender = table.Column<string>(maxLength: 20, nullable: false),
                    CoverLetter = table.Column<string>(maxLength: 2000, nullable: false),
                    ResumePath = table.Column<string>(maxLength: 1000, nullable: true),
                    TermsAccepted = table.Column<bool>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_AspNetUsers_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobPostingId",
                table: "Applications",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StudentUserId",
                table: "Applications",
                column: "StudentUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
