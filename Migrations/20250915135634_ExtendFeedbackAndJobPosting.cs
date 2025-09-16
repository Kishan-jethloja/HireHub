using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlacementManagementSystem.Migrations
{
    public partial class ExtendFeedbackAndJobPosting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorUserId = table.Column<string>(nullable: false),
                    TargetType = table.Column<int>(nullable: false),
                    TargetCompanyId = table.Column<int>(nullable: true),
                    TargetCollegeName = table.Column<string>(maxLength: 200, nullable: true),
                    Subject = table.Column<string>(maxLength: 150, nullable: false),
                    Message = table.Column<string>(maxLength: 2000, nullable: false),
                    Rating = table.Column<int>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_AspNetUsers_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Companies_TargetCompanyId",
                        column: x => x.TargetCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyUserId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    CollegeName = table.Column<string>(maxLength: 200, nullable: false),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    GoogleFormUrl = table.Column<string>(maxLength: 1000, nullable: true),
                    Location = table.Column<string>(maxLength: 200, nullable: true),
                    Compensation = table.Column<int>(nullable: true),
                    ApplyByUtc = table.Column<DateTime>(nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPostings_AspNetUsers_CompanyUserId",
                        column: x => x.CompanyUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_AuthorUserId",
                table: "Feedbacks",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_TargetCompanyId",
                table: "Feedbacks",
                column: "TargetCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_CompanyUserId",
                table: "JobPostings",
                column: "CompanyUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "JobPostings");
        }
    }
}
