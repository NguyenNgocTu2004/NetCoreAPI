using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddKhoaAndStudentKhoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KhoaId",
                table: "Students",
                type: "INTEGER",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Students_KhoaId",
                table: "Students",
                column: "KhoaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Khoas_KhoaId",
                table: "Students",
                column: "KhoaId",
                principalTable: "Khoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Khoas_KhoaId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Khoas");

            migrationBuilder.DropIndex(
                name: "IX_Students_KhoaId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "KhoaId",
                table: "Students");
        }
    }
}
