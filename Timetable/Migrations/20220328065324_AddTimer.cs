using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timetable.Migrations
{
    public partial class AddTimer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "timer",
                table: "Users",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timer",
                table: "Users");
        }
    }
}
