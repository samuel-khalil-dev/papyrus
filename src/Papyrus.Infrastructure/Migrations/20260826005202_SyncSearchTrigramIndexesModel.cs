using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papyrus.Infrastructure.Migrations
{
    /// <summary>Aligns the model-managed trigram index names with the indexes created by the previous migration.</summary>
    public partial class SyncSearchTrigramIndexesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_Projects_Search_Description\" RENAME TO \"IX_Projects_Description\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_Projects_Search_Summary\" RENAME TO \"IX_Projects_Summary\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_Projects_Search_Title\" RENAME TO \"IX_Projects_Title\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_ContentTags_Search_Name\" RENAME TO \"IX_ContentTags_Name\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_BlogPosts_Search_ContentMarkdown\" RENAME TO \"IX_BlogPosts_ContentMarkdown\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_BlogPosts_Search_Summary\" RENAME TO \"IX_BlogPosts_Summary\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_BlogPosts_Search_Title\" RENAME TO \"IX_BlogPosts_Title\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_Projects_Description\" RENAME TO \"IX_Projects_Search_Description\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_Projects_Summary\" RENAME TO \"IX_Projects_Search_Summary\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_Projects_Title\" RENAME TO \"IX_Projects_Search_Title\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_ContentTags_Name\" RENAME TO \"IX_ContentTags_Search_Name\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_BlogPosts_ContentMarkdown\" RENAME TO \"IX_BlogPosts_Search_ContentMarkdown\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_BlogPosts_Summary\" RENAME TO \"IX_BlogPosts_Search_Summary\";");
            migrationBuilder.Sql("ALTER INDEX IF EXISTS \"IX_BlogPosts_Title\" RENAME TO \"IX_BlogPosts_Search_Title\";");
        }
    }
}
