using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papyrus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3PersistenceCorrectness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_ContentCategories_CategoryId",
                table: "BlogPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_Projects_ProjectId",
                table: "BlogPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTags_BlogPosts_BlogPostId",
                table: "BlogPostTags");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTags_ContentTags_TagId",
                table: "BlogPostTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ContentCategories_CategoryId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTags_ContentTags_TagId",
                table: "ProjectTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTags_Projects_ProjectId",
                table: "ProjectTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectTags",
                table: "ProjectTags");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTags_TagId",
                table: "ProjectTags");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTags_TenantId",
                table: "ProjectTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CategoryId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_CategoryId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_IsPublished_IsFeatured",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_Slug",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentTags",
                table: "ContentTags");

            migrationBuilder.DropIndex(
                name: "IX_ContentTags_TenantId_Slug",
                table: "ContentTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentCategories",
                table: "ContentCategories");

            migrationBuilder.DropIndex(
                name: "IX_ContentCategories_TenantId_IsPublished",
                table: "ContentCategories");

            migrationBuilder.DropIndex(
                name: "IX_ContentCategories_TenantId_Slug",
                table: "ContentCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPostTags",
                table: "BlogPostTags");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTags_TagId",
                table: "BlogPostTags");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTags_TenantId",
                table: "BlogPostTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPosts",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_CategoryId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_ProjectId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_CategoryId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_IsPublished_PublishedAt",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_ProjectId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_Slug",
                table: "BlogPosts");

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "Projects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "ContentTags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ContentTags",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "ContentTags",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "ContentTags",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ContentTags",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContentTags",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ContentTags",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "ContentCategories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ContentCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "ContentCategories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "ContentCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ContentCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContentCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ContentCategories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "BlogPosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BlogPosts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "BlogPosts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "BlogPosts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BlogPosts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BlogPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "BlogPosts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectTags",
                table: "ProjectTags",
                columns: new[] { "TenantId", "ProjectId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentTags",
                table: "ContentTags",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentCategories",
                table: "ContentCategories",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPostTags",
                table: "BlogPostTags",
                columns: new[] { "TenantId", "BlogPostId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPosts",
                table: "BlogPosts",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_TenantId_ProjectId",
                table: "ProjectTags",
                columns: new[] { "TenantId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_TenantId_TagId",
                table: "ProjectTags",
                columns: new[] { "TenantId", "TagId" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_CategoryId",
                table: "Projects",
                columns: new[] { "TenantId", "CategoryId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_IsPublished_IsFeatured_PublishedAt",
                table: "Projects",
                columns: new[] { "TenantId", "IsPublished", "IsFeatured", "PublishedAt" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_Slug",
                table: "Projects",
                columns: new[] { "TenantId", "Slug" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_TenantId_NotEmpty",
                table: "Projects",
                sql: "\"TenantId\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_ContentTags_TenantId_Slug",
                table: "ContentTags",
                columns: new[] { "TenantId", "Slug" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContentTags_TenantId_NotEmpty",
                table: "ContentTags",
                sql: "\"TenantId\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCategories_TenantId_IsPublished",
                table: "ContentCategories",
                columns: new[] { "TenantId", "IsPublished" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCategories_TenantId_Slug",
                table: "ContentCategories",
                columns: new[] { "TenantId", "Slug" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContentCategories_TenantId_NotEmpty",
                table: "ContentCategories",
                sql: "\"TenantId\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_TenantId_BlogPostId",
                table: "BlogPostTags",
                columns: new[] { "TenantId", "BlogPostId" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_TenantId_TagId",
                table: "BlogPostTags",
                columns: new[] { "TenantId", "TagId" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_CategoryId",
                table: "BlogPosts",
                columns: new[] { "TenantId", "CategoryId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_IsPublished_PublishedAt",
                table: "BlogPosts",
                columns: new[] { "TenantId", "IsPublished", "PublishedAt" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_ProjectId",
                table: "BlogPosts",
                columns: new[] { "TenantId", "ProjectId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_Slug",
                table: "BlogPosts",
                columns: new[] { "TenantId", "Slug" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BlogPosts_TenantId_NotEmpty",
                table: "BlogPosts",
                sql: "\"TenantId\" <> ''");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_ContentCategories_TenantId_CategoryId",
                table: "BlogPosts",
                columns: new[] { "TenantId", "CategoryId" },
                principalTable: "ContentCategories",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_Projects_TenantId_ProjectId",
                table: "BlogPosts",
                columns: new[] { "TenantId", "ProjectId" },
                principalTable: "Projects",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTags_BlogPosts_TenantId_BlogPostId",
                table: "BlogPostTags",
                columns: new[] { "TenantId", "BlogPostId" },
                principalTable: "BlogPosts",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTags_ContentTags_TenantId_TagId",
                table: "BlogPostTags",
                columns: new[] { "TenantId", "TagId" },
                principalTable: "ContentTags",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ContentCategories_TenantId_CategoryId",
                table: "Projects",
                columns: new[] { "TenantId", "CategoryId" },
                principalTable: "ContentCategories",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTags_ContentTags_TenantId_TagId",
                table: "ProjectTags",
                columns: new[] { "TenantId", "TagId" },
                principalTable: "ContentTags",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTags_Projects_TenantId_ProjectId",
                table: "ProjectTags",
                columns: new[] { "TenantId", "ProjectId" },
                principalTable: "Projects",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_ContentCategories_TenantId_CategoryId",
                table: "BlogPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_Projects_TenantId_ProjectId",
                table: "BlogPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTags_BlogPosts_TenantId_BlogPostId",
                table: "BlogPostTags");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTags_ContentTags_TenantId_TagId",
                table: "BlogPostTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ContentCategories_TenantId_CategoryId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTags_ContentTags_TenantId_TagId",
                table: "ProjectTags");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTags_Projects_TenantId_ProjectId",
                table: "ProjectTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectTags",
                table: "ProjectTags");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTags_TenantId_ProjectId",
                table: "ProjectTags");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTags_TenantId_TagId",
                table: "ProjectTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_CategoryId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_IsPublished_IsFeatured_PublishedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_Slug",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_TenantId_NotEmpty",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentTags",
                table: "ContentTags");

            migrationBuilder.DropIndex(
                name: "IX_ContentTags_TenantId_Slug",
                table: "ContentTags");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContentTags_TenantId_NotEmpty",
                table: "ContentTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentCategories",
                table: "ContentCategories");

            migrationBuilder.DropIndex(
                name: "IX_ContentCategories_TenantId_IsPublished",
                table: "ContentCategories");

            migrationBuilder.DropIndex(
                name: "IX_ContentCategories_TenantId_Slug",
                table: "ContentCategories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContentCategories_TenantId_NotEmpty",
                table: "ContentCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPostTags",
                table: "BlogPostTags");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTags_TenantId_BlogPostId",
                table: "BlogPostTags");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTags_TenantId_TagId",
                table: "BlogPostTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPosts",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_CategoryId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_IsPublished_PublishedAt",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_ProjectId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_TenantId_Slug",
                table: "BlogPosts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BlogPosts_TenantId_NotEmpty",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ContentTags");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ContentCategories");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "BlogPosts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectTags",
                table: "ProjectTags",
                columns: new[] { "ProjectId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentTags",
                table: "ContentTags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentCategories",
                table: "ContentCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPostTags",
                table: "BlogPostTags",
                columns: new[] { "BlogPostId", "TagId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPosts",
                table: "BlogPosts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_TagId",
                table: "ProjectTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_TenantId",
                table: "ProjectTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CategoryId",
                table: "Projects",
                column: "CategoryId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ContentTags_TenantId_Slug",
                table: "ContentTags",
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
                name: "IX_BlogPostTags_TagId",
                table: "BlogPostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_TenantId",
                table: "BlogPostTags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CategoryId",
                table: "BlogPosts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_ProjectId",
                table: "BlogPosts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_CategoryId",
                table: "BlogPosts",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_IsPublished_PublishedAt",
                table: "BlogPosts",
                columns: new[] { "TenantId", "IsPublished", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_ProjectId",
                table: "BlogPosts",
                columns: new[] { "TenantId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_TenantId_Slug",
                table: "BlogPosts",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_ContentCategories_CategoryId",
                table: "BlogPosts",
                column: "CategoryId",
                principalTable: "ContentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_Projects_ProjectId",
                table: "BlogPosts",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTags_BlogPosts_BlogPostId",
                table: "BlogPostTags",
                column: "BlogPostId",
                principalTable: "BlogPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTags_ContentTags_TagId",
                table: "BlogPostTags",
                column: "TagId",
                principalTable: "ContentTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ContentCategories_CategoryId",
                table: "Projects",
                column: "CategoryId",
                principalTable: "ContentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTags_ContentTags_TagId",
                table: "ProjectTags",
                column: "TagId",
                principalTable: "ContentTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTags_Projects_ProjectId",
                table: "ProjectTags",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
