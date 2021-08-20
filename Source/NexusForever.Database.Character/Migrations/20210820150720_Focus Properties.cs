using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Character.Migrations
{
    public partial class FocusProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 17u, 0u, 0u },
                column: "note",
                value: "Class - Warrior - Base Kinetic Energy Regen");

            migrationBuilder.InsertData(
                table: "property_base",
                columns: new[] { "property", "subtype", "type", "modType", "note", "value" },
                values: new object[,]
                {
                    { 5u, 4u, 1u, (ushort)2, "Class - Medic - Base Focus Pool", 1000f },
                    { 5u, 7u, 1u, (ushort)2, "Class - Spellslinger - Base Focus Pool", 1000f },
                    { 5u, 3u, 1u, (ushort)2, "Class - Esper - Base Focus Pool", 1000f },
                    { 107u, 4u, 1u, (ushort)2, "Class - Medic - Base Focus Recovery Rate In Combat", 0.005f },
                    { 107u, 7u, 1u, (ushort)2, "Class - Spellslinger - Focus Recovery Rate In Combat", 0.005f },
                    { 107u, 3u, 1u, (ushort)2, "Class - Esper - Focus Recovery Rate In Combat", 0.005f },
                    { 108u, 4u, 1u, (ushort)2, "Class - Medic - Base Focus Recovery Rate Out of Combat", 0.02f },
                    { 108u, 7u, 1u, (ushort)2, "Class - Spellslinger - Focus Recovery Rate Out of Combat", 0.02f },
                    { 108u, 3u, 1u, (ushort)2, "Class - Esper - Focus Recovery Rate Out of Combat", 0.02f }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 5u, 3u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 107u, 3u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 108u, 3u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 5u, 4u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 107u, 4u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 108u, 4u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 5u, 7u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 107u, 7u, 1u });

            migrationBuilder.DeleteData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 108u, 7u, 1u });

            migrationBuilder.UpdateData(
                table: "property_base",
                keyColumns: new[] { "property", "subtype", "type" },
                keyValues: new object[] { 17u, 0u, 0u },
                column: "note",
                value: "Warrior - Base Kinetic Energy Regen");
        }
    }
}
