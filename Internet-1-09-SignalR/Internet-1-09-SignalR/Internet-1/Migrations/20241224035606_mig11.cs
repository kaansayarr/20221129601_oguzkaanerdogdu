using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internet_1.Migrations
{
    /// <inheritdoc />
    public partial class mig11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Videourl",
                table: "Lessons",
                newName: "VideoUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "Lessons",
                newName: "Videourl");
        }
    }
}
