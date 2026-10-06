using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TacticalHeroes.Admin.Infrastructure.DataProtection.Migrations;

/// <inheritdoc />
public partial class CreateDataProtectionKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "admin");

        migrationBuilder.CreateTable(
            name: "data_protection_keys",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                friendly_name = table.Column<string>(type: "text", nullable: true),
                xml = table.Column<string>(type: "text", nullable: true)
            },
            schema: "admin",
            constraints: table =>
            {
                table.PrimaryKey("pk_data_protection_keys", x => x.id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "data_protection_keys",
            schema: "admin");
    }
}
