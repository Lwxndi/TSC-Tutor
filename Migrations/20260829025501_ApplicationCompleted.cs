using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutor_Manager.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId1",
                table: "TutorSubjects");

            migrationBuilder.DropIndex(
                name: "IX_TutorSubjects_SubjectId1",
                table: "TutorSubjects");

            migrationBuilder.DropColumn(
                name: "SubjectId1",
                table: "TutorSubjects");

            migrationBuilder.CreateTable(
                name: "AccountActivationTokens",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountActivationTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_AccountActivationTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountActivationTokens_UserId",
                table: "AccountActivationTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountActivationTokens");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId1",
                table: "TutorSubjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjects_SubjectId1",
                table: "TutorSubjects",
                column: "SubjectId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorSubjects_Subjects_SubjectId1",
                table: "TutorSubjects",
                column: "SubjectId1",
                principalTable: "Subjects",
                principalColumn: "SubjectId");
        }
    }
}
