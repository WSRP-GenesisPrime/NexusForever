using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Character.Migrations
{
    public partial class DecorEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "hookBagIndex",
                table: "residence_decor",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "hookIndex",
                table: "residence_decor",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hookBagIndex",
                table: "residence_decor");

            migrationBuilder.DropColumn(
                name: "hookIndex",
                table: "residence_decor");
        }
    }
}
