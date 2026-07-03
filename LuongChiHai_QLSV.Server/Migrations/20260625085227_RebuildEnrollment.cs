using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuongChiHai_QLSV.Server.Migrations
{
    /// <inheritdoc />
    public partial class RebuildEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseID);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "varchar(15)", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.StudentID);
                });

            migrationBuilder.CreateTable(
                name: "AcademicProfile",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "varchar(15)", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CampusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EducationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FacultyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MajorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecializationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcademicYear = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Đang học")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicProfile", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_AcademicProfile_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Semester = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProcessScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    MidtermScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    FinalExamScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    WeightProcess = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.20m),
                    WeightMidterm = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.30m),
                    WeightFinal = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.50m),
                    TotalScore = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    IsPassed = table.Column<bool>(type: "bit", nullable: true),
                    CourseID1 = table.Column<int>(type: "int", nullable: true),
                    StudentID1 = table.Column<string>(type: "varchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => new { x.StudentID, x.CourseID });
                    table.ForeignKey(
                        name: "FK_Enrollment_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_Course_CourseID1",
                        column: x => x.CourseID1,
                        principalTable: "Course",
                        principalColumn: "CourseID");
                    table.ForeignKey(
                        name: "FK_Enrollment_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_Student_StudentID1",
                        column: x => x.StudentID1,
                        principalTable: "Student",
                        principalColumn: "StudentID");
                });

            migrationBuilder.CreateTable(
                name: "FamilyRelationship",
                columns: table => new
                {
                    FamilyMemberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<string>(type: "varchar(15)", nullable: false),
                    RelativeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    BirthYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRelationship", x => x.FamilyMemberID);
                    table.ForeignKey(
                        name: "FK_FamilyRelationship_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfile",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "varchar(15)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ethnicity = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthPlace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CitizenID = table.Column<string>(type: "char(12)", maxLength: 12, nullable: true),
                    CitizenIDIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CitizenIDIssuePlace = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PermanentAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TemporaryAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfile", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_StudentProfile_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_CourseCode",
                table: "Course",
                column: "CourseCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseID",
                table: "Enrollment",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseID1",
                table: "Enrollment",
                column: "CourseID1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_StudentID1",
                table: "Enrollment",
                column: "StudentID1");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRelationship_StudentID",
                table: "FamilyRelationship",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfile_CitizenID",
                table: "StudentProfile",
                column: "CitizenID",
                unique: true,
                filter: "[CitizenID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfile_Email",
                table: "StudentProfile",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfile_PhoneNumber",
                table: "StudentProfile",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicProfile");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "FamilyRelationship");

            migrationBuilder.DropTable(
                name: "StudentProfile");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Student");
        }
    }
}
