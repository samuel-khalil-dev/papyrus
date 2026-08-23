using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papyrus.Infrastructure.Migrations;

/// <summary>Indexes the bounded ILIKE search fields without changing existing migrations.</summary>
public partial class SecuritySearchTrigramIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_BlogPosts_Search_Title\" ON \"BlogPosts\" USING gin (\"Title\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_BlogPosts_Search_Summary\" ON \"BlogPosts\" USING gin (\"Summary\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_BlogPosts_Search_ContentMarkdown\" ON \"BlogPosts\" USING gin (\"ContentMarkdown\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Projects_Search_Title\" ON \"Projects\" USING gin (\"Title\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Projects_Search_Summary\" ON \"Projects\" USING gin (\"Summary\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Projects_Search_Description\" ON \"Projects\" USING gin (\"Description\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_ContentTags_Search_Name\" ON \"ContentTags\" USING gin (\"Name\" gin_trgm_ops) WHERE \"IsDeleted\" = false;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_BlogPosts_Search_Title\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_BlogPosts_Search_Summary\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_BlogPosts_Search_ContentMarkdown\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Projects_Search_Title\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Projects_Search_Summary\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Projects_Search_Description\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_ContentTags_Search_Name\";");
    }
}
