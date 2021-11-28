using Microsoft.EntityFrameworkCore.Migrations;

namespace NexusForever.Database.Auth.Migrations
{
    public partial class PlayerRolePermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "role_permission",
                columns: new[] { "id", "permissionId" },
                values: new object[,]
                {
                    { 1u, 4u },
                    { 1u, 43u },
                    { 1u, 44u },
                    { 1u, 46u },
                    { 1u, 47u },
                    { 1u, 48u },
                    { 1u, 49u },
                    { 1u, 50u },
                    { 1u, 51u },
                    { 1u, 52u },
                    { 1u, 53u },
                    { 1u, 54u },
                    { 1u, 55u },
                    { 1u, 56u },
                    { 1u, 57u },
                    { 1u, 58u },
                    { 1u, 59u },
                    { 1u, 6u },
                    { 1u, 92u },
                    { 1u, 76u },
                    { 1u, 42u },
                    { 1u, 110u },
                    { 1u, 41u },
                    { 1u, 39u },
                    { 1u, 1u },
                    { 1u, 112u },
                    { 1u, 20u },
                    { 1u, 21u },
                    { 1u, 22u },
                    { 1u, 5u },
                    { 1u, 23u },
                    { 1u, 24u },
                    { 1u, 25u },
                    { 1u, 26u },
                    { 1u, 27u },
                    { 1u, 28u },
                    { 1u, 29u },
                    { 1u, 33u },
                    { 1u, 34u },
                    { 1u, 35u },
                    { 1u, 36u },
                    { 1u, 37u },
                    { 1u, 38u },
                    { 1u, 40u },
                    { 1u, 78u }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 1u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 4u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 5u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 6u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 20u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 21u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 22u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 23u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 24u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 25u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 26u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 27u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 28u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 29u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 33u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 34u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 35u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 36u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 37u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 38u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 39u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 40u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 41u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 42u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 43u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 44u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 46u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 47u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 48u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 49u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 50u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 51u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 52u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 53u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 54u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 55u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 56u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 57u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 58u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 59u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 76u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 78u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 92u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 110u });

            migrationBuilder.DeleteData(
                table: "role_permission",
                keyColumns: new[] { "id", "permissionId" },
                keyValues: new object[] { 1u, 112u });
        }
    }
}
