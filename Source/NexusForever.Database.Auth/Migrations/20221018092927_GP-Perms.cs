using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Auth.Migrations
{
    public partial class GPPerms : Migration
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
                    { 50710u, "Command: RealmUptime" },
                    { 50900u, "Flag: GM" },
                    { 60000u, "Category: Costume" },
                    { 60010u, "Command: CostumeOverride" },
                    { 60020u, "Command: CostumeOverrideId" },
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
                    { 6u, 1u, "Storyteller" },
                    { 7u, 1u, "NSFWS" },
                    { 8u, 1u, "NFSWSMod" }
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

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 50900u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 60000u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 60010u);

            migrationBuilder.DeleteData(
                table: "permission",
                keyColumn: "id",
                keyValue: 60020u);

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
                keyValue: 6u);

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
