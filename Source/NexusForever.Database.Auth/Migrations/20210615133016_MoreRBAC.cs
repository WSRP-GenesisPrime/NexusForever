using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Auth.Migrations
{
    public partial class MoreRBAC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permission",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 50000u, "Category: Morph" },
                    { 50001u, "Command: MorphStoryTeller" },
                    { 50100u, "Category: Emote" },
                    { 50200u, "Category: Chron" },
                    { 50300u, "Category: XRoll" },
                    { 50400u, "Command: CharacterProps" },
                    { 50500u, "Category: Boost" },
                    { 50600u, "Command: HouseRemodel" },
                    { 50700u, "Command: RealmOnline" },
                    { 50710u, "Command: RealmUptime" }
                });

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "name", "flags" },
                values: new object[,]
                {
                    { 6u, "Storyteller", 1u }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50000u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50001u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50100u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50200u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50300u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50400u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50500u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50600u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50700u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50710u);
        }
    }
}
