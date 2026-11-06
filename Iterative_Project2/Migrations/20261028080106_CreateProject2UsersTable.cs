using Microsoft.EntityFrameworkCore.Migrations;

namespace Iterative_Project2.Migrations
{
    public partial class CreateProject2UsersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Project2");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "Project2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "Project2",
                newName: "Users");
        }
    }
}
