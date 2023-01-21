using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Auth.Migrations
{
    /// <inheritdoc />
    public partial class AccountLinkModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_link",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "0")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account_link_entry",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "0")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accountId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_link_entry", x => new { x.id, x.accountId });
                    table.ForeignKey(
                        name: "FK__account_link_account_id__account_id",
                        column: x => x.accountId,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__account_link_id__link_id",
                        column: x => x.id,
                        principalTable: "account_link",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_account_link_entry_accountId",
                table: "account_link_entry",
                column: "accountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_link_entry");

            migrationBuilder.DropTable(
                name: "account_link");
        }
    }
}
