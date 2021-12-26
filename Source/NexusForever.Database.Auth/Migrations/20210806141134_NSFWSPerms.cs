using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Auth.Migrations
{
    public partial class NSFWSPerms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 61000u, "Flag: Adult" },
                    { 61100u, "Command: AdultPlotLockSelf" },
                    { 61150u, "Command: AdultPlotLockNonOwner" },
                    { 61200u, "Command: AdultPlotAlert" }
                });

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "flags", "name" },
                values: new object[,]
                {
                    { 7u, 1u, "NSFWS" },
                    { 8u, 1u, "NFSWSMod" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 61000u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 61100u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 61150u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 61200u);

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: 7u);

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: 8u);
        }
    }
}
