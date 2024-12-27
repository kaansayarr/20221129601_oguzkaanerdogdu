using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internet_1.Migrations
{
    /// <inheritdoc />
    public partial class tedsahdas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VıdeoID",
                table: "Lessons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VıdeoID",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
