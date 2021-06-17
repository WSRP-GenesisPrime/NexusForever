using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Auth.Migrations
{
    public partial class AddStoryTellerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "flags", "name" },
                values: new object[] { 6u, 1u, "Storyteller" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: 6u);
        }
    }
}
