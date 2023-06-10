using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.World.Migrations
{
    /// <inheritdoc />
    public partial class RealmTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "realm_task",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u, comment: "Realm Task ID"),
                    type = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false, defaultValue: (byte)0),
                    value = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    characterId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u, comment: "Character ID"),
                    accountId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u, comment: "Account ID"),
                    guildId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u, comment: "Guild ID"),
                    referenceId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    referenceValue = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false, defaultValue: (byte)0),
                    statusDescription = table.Column<string>(type: "varchar(4000)", nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    createTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()"),
                    createdBy = table.Column<string>(type: "varchar(128)", nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastRunTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "current_timestamp()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "realm_task");
        }
    }
}
