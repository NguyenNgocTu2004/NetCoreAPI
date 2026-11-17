using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddClassesSubjectsGrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH RankedStudents AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY StudentCode ORDER BY Id) AS DuplicateRank
                    FROM Students
                )
                UPDATE Students
                SET StudentCode = printf('%010d', 9000000000 + Id)
                WHERE Id IN (
                    SELECT Id
                    FROM RankedStudents
                    WHERE DuplicateRank > 1
                );
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Khoas_KhoaId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Khoas");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.RenameColumn(
                name: "KhoaId",
                table: "Students",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_KhoaId",
                table: "Students",
                newName: "IX_Students_ClassId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Students",
                type: "TEXT",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GioiTinh",
                table: "Students",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgaySinh",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaLop = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenLop = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NganhHoc = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KhoaHoc = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaMonHoc = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenMonHoc = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SoTinChi = table.Column<int>(type: "INTEGER", nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiemQuaTrinh = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    DiemThi = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    DiemTongKet = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    XepLoai = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grades_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentCode",
                table: "Students",
                column: "StudentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_MaLop",
                table: "Classes",
                column: "MaLop",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId_SubjectId",
                table: "Grades",
                columns: new[] { "StudentId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubjectId",
                table: "Grades",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_MaMonHoc",
                table: "Subjects",
                column: "MaMonHoc",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Classes_ClassId",
                table: "Students",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Classes_ClassId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GioiTinh",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "NgaySinh",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "Students",
                newName: "KhoaId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                newName: "IX_Students_KhoaId");

            migrationBuilder.CreateTable(
                name: "Khoas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaKhoa = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TenKhoa = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChucVu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Luong = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaNhanVien = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PhongBan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Khoas_KhoaId",
                table: "Students",
                column: "KhoaId",
                principalTable: "Khoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
