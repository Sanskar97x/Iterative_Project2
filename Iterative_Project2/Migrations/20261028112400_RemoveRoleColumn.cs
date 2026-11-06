using Microsoft.EntityFrameworkCore.Migrations;

namespace Iterative_Project2.Migrations
{
    public partial class RemoveRoleColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                schema: "Project2",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "Project2",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
