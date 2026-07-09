using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManteqTask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDoctorUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "email", "first_name", "is_active", "is_verified", "last_name", "password_hash", "security_stamp", "updated_at", "updated_by", "username" },
                values: new object[] { new Guid("d0000000-0000-7000-8000-000000000001"), new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a0000000-0000-7000-8000-000000000000"), null, null, "doctor@example.com", "Doctor", true, true, "User", "5658A3510D0C8BA1DFD6AF62A44E06736E0B8E43B25464887D6007E5688C7270-7F8784D082093474FEED885A5F977C20", "0199ecd5-1111-7111-8111-111111111111", null, null, "doctor" });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { new Guid("77777777-7777-7777-8777-777777777777"), new Guid("d0000000-0000-7000-8000-000000000001") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "user_roles",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { new Guid("77777777-7777-7777-8777-777777777777"), new Guid("d0000000-0000-7000-8000-000000000001") });

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("d0000000-0000-7000-8000-000000000001"));
        }
    }
}
