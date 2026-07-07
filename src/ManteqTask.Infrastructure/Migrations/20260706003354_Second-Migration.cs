using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManteqTask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-7111-8111-111111111111"),
                column: "SecurityStamp",
                value: "0199ecd4-f5b6-7211-9ec7-ce26d0966b72");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-7000-8000-000000000000"),
                column: "SecurityStamp",
                value: "0199ecd3-b844-792f-8f83-431df66c629d");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-7111-8111-111111111111"),
                column: "SecurityStamp",
                value: "10/15/2025 00:00:00");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-7000-8000-000000000000"),
                column: "SecurityStamp",
                value: "10/15/2025 00:00:00");
        }
    }
}
