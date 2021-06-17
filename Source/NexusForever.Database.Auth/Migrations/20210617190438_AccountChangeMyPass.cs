using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Auth.Migrations
{
    public partial class AccountChangeMyPass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "name" },
                values: new object[] { 50810u, "Command: AccountChangeMyPass" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50810u);
        }
    }
}
