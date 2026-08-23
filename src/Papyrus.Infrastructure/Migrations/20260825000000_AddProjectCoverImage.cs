using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papyrus.Infrastructure.Migrations;

public partial class AddProjectCoverImage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CoverImageUrl",
            table: "Projects",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CoverImageUrl",
            table: "Projects");
    }
}
