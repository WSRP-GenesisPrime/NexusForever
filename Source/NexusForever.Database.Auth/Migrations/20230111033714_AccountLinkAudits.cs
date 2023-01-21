using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Auth.Migrations
{
    /// <inheritdoc />
    public partial class AccountLinkAudits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "createTime",
                table: "account_link",
                type: "datetime",
                nullable: false,
                defaultValueSql: "current_timestamp()");

            migrationBuilder.AddColumn<string>(
                name: "createdBy",
                table: "account_link",
                type: "varchar(128)",
                nullable: true,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createTime",
                table: "account_link");

            migrationBuilder.DropColumn(
                name: "createdBy",
                table: "account_link");
        }
    }
}
