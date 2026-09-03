using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutor_Manager.Migrations
{
    /// <inheritdoc />
    public partial class RenameSubjectToMathLiteracy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldOfStudy",
                table: "TutorApplications");

            migrationBuilder.DropColumn(
                name: "HighestQualification",
                table: "TutorApplications");

            migrationBuilder.DropColumn(
                name: "Institution",
                table: "TutorApplications");

            migrationBuilder.DropColumn(
                name: "StudyStatus",
                table: "TutorApplications");

            migrationBuilder.DropColumn(
                name: "YearCompleted",
                table: "TutorApplications");

            migrationBuilder.CreateTable(
                name: "TutorApplicationQualifications",
                columns: table => new
                {
                    QualificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    QualificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    YearCompleted = table.Column<int>(type: "int", nullable: true),
                    StudyStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplicationQualifications", x => x.QualificationId);
                    table.ForeignKey(
                        name: "FK_TutorApplicationQualifications_TutorApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "TutorApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "SubjectId",
                keyValue: 4,
                column: "SubjectName",
                value: "Mathematical Literacy");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplicationQualifications_ApplicationId",
                table: "TutorApplicationQualifications",
                column: "ApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorApplicationQualifications");

            migrationBuilder.AddColumn<string>(
                name: "FieldOfStudy",
                table: "TutorApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighestQualification",
                table: "TutorApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Institution",
                table: "TutorApplications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudyStatus",
                table: "TutorApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearCompleted",
                table: "TutorApplications",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "SubjectId",
                keyValue: 4,
                column: "SubjectName",
                value: "English Home Language");
        }
    }
}
