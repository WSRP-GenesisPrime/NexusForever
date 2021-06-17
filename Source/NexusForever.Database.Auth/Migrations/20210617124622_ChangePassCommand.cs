using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Auth.Migrations
{
    public partial class ChangePassCommand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "name" },
                values: new object[] { 50800u, "Command: AccountChangePass" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50800u);
        }
    }
}
