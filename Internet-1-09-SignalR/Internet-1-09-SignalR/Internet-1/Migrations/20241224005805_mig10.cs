using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internet_1.Migrations
{
    /// <inheritdoc />
    public partial class mig10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VideoId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VıdeoID",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_InstructorId",
                table: "Lessons",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_VideoId",
                table: "Lessons",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Instructors_InstructorId",
                table: "Lessons",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Videos_VideoId",
                table: "Lessons",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Instructors_InstructorId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Videos_VideoId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_InstructorId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_VideoId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VıdeoID",
                table: "Lessons");
        }
    }
}
