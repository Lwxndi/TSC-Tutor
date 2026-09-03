using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutor_Manager.Migrations
{
    /// <inheritdoc />
    public partial class TutorSubjectNoActionCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId",
                table: "TutorSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorSubjects_Tutors_TutorUserId",
                table: "TutorSubjects");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId1",
                table: "TutorSubjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "Tutors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TutorNumber",
                table: "Tutors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TutorApplications",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    AltPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    LocationPreference = table.Column<int>(type: "int", nullable: false),
                    AreaCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HighestQualification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Institution = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    YearCompleted = table.Column<int>(type: "int", nullable: true),
                    StudyStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Achievements = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StrengthsSelected = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StrengthsNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ConsentGiven = table.Column<bool>(type: "bit", nullable: false),
                    ConsentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsentVersion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateApplied = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateReviewed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    CreatedTutorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_TutorApplications_Tutors_CreatedTutorId",
                        column: x => x.CreatedTutorId,
                        principalTable: "Tutors",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_TutorApplications_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_TutorApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "TutorApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorApplicationExperiences",
                columns: table => new
                {
                    ExperienceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SubjectsTaught = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GradeLevels = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Responsibilities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Achievements = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplicationExperiences", x => x.ExperienceId);
                    table.ForeignKey(
                        name: "FK_TutorApplicationExperiences_TutorApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "TutorApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorApplicationSubjects",
                columns: table => new
                {
                    TutorApplicationSubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    GradeLevels = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompetencyNote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResultNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplicationSubjects", x => x.TutorApplicationSubjectId);
                    table.ForeignKey(
                        name: "FK_TutorApplicationSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TutorApplicationSubjects_TutorApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "TutorApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjects_SubjectId1",
                table: "TutorSubjects",
                column: "SubjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_ApplicationId",
                table: "ApplicationDocuments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplicationExperiences_ApplicationId",
                table: "TutorApplicationExperiences",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplications_CreatedTutorId",
                table: "TutorApplications",
                column: "CreatedTutorId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplications_ReferenceNumber",
                table: "TutorApplications",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplications_ReviewedByAdminId",
                table: "TutorApplications",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplicationSubjects_ApplicationId",
                table: "TutorApplicationSubjects",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplicationSubjects_SubjectId",
                table: "TutorApplicationSubjects",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId",
                table: "TutorSubjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId1",
                table: "TutorSubjects",
                column: "SubjectId1",
                principalTable: "Subjects",
                principalColumn: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSubjects_Tutors_TutorUserId",
                table: "TutorSubjects",
                column: "TutorUserId",
                principalTable: "Tutors",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId",
                table: "TutorSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId1",
                table: "TutorSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorSubjects_Tutors_TutorUserId",
                table: "TutorSubjects");

            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "TutorApplicationExperiences");

            migrationBuilder.DropTable(
                name: "TutorApplicationSubjects");

            migrationBuilder.DropTable(
                name: "TutorApplications");

            migrationBuilder.DropIndex(
                name: "IX_TutorSubjects_SubjectId1",
                table: "TutorSubjects");

            migrationBuilder.DropColumn(
                name: "SubjectId1",
                table: "TutorSubjects");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Tutors");

            migrationBuilder.DropColumn(
                name: "TutorNumber",
                table: "Tutors");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId",
                table: "TutorSubjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSubjects_Tutors_TutorUserId",
                table: "TutorSubjects",
                column: "TutorUserId",
                principalTable: "Tutors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
