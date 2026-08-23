using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papyrus.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ContentCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContentCategories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContentTags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContentTags", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                RepositoryUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                LiveUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Projects", x => x.Id);
                table.ForeignKey(
                    name: "FK_Projects_ContentCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "ContentCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "BlogPosts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                ContentMarkdown = table.Column<string>(type: "text", nullable: false),
                CoverImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BlogPosts", x => x.Id);
                table.ForeignKey(
                    name: "FK_BlogPosts_ContentCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "ContentCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_BlogPosts_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "ProjectTags",
            columns: table => new
            {
                ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                TagId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectTags", x => new { x.ProjectId, x.TagId });
                table.ForeignKey(
                    name: "FK_ProjectTags_ContentTags_TagId",
                    column: x => x.TagId,
                    principalTable: "ContentTags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProjectTags_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BlogPostTags",
            columns: table => new
            {
                BlogPostId = table.Column<Guid>(type: "uuid", nullable: false),
                TagId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BlogPostTags", x => new { x.BlogPostId, x.TagId });
                table.ForeignKey(
                    name: "FK_BlogPostTags_BlogPosts_BlogPostId",
                    column: x => x.BlogPostId,
                    principalTable: "BlogPosts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BlogPostTags_ContentTags_TagId",
                    column: x => x.TagId,
                    principalTable: "ContentTags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BlogPostTags_TenantId",
            table: "BlogPostTags",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_BlogPostTags_TagId",
            table: "BlogPostTags",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_BlogPosts_TenantId_CategoryId",
            table: "BlogPosts",
            columns: new[] { "TenantId", "CategoryId" });

        migrationBuilder.CreateIndex(
            name: "IX_BlogPosts_TenantId_IsPublished_PublishedAt",
            table: "BlogPosts",
            columns: new[] { "TenantId", "IsPublished", "PublishedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_BlogPosts_CategoryId",
            table: "BlogPosts",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_BlogPosts_ProjectId",
            table: "BlogPosts",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_BlogPosts_TenantId_ProjectId",
            table: "BlogPosts",
            columns: new[] { "TenantId", "ProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_BlogPosts_TenantId_Slug",
            table: "BlogPosts",
            columns: new[] { "TenantId", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContentCategories_TenantId_IsPublished",
            table: "ContentCategories",
            columns: new[] { "TenantId", "IsPublished" });

        migrationBuilder.CreateIndex(
            name: "IX_ContentCategories_TenantId_Slug",
            table: "ContentCategories",
            columns: new[] { "TenantId", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContentTags_TenantId_Slug",
            table: "ContentTags",
            columns: new[] { "TenantId", "Slug" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Projects_CategoryId",
            table: "Projects",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTags_TenantId",
            table: "ProjectTags",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTags_TagId",
            table: "ProjectTags",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TenantId_CategoryId",
            table: "Projects",
            columns: new[] { "TenantId", "CategoryId" });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TenantId_IsPublished_IsFeatured",
            table: "Projects",
            columns: new[] { "TenantId", "IsPublished", "IsFeatured" });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_TenantId_Slug",
            table: "Projects",
            columns: new[] { "TenantId", "Slug" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "BlogPostTags");
        migrationBuilder.DropTable(name: "ProjectTags");
        migrationBuilder.DropTable(name: "BlogPosts");
        migrationBuilder.DropTable(name: "ContentTags");
        migrationBuilder.DropTable(name: "Projects");
        migrationBuilder.DropTable(name: "ContentCategories");
    }
}
