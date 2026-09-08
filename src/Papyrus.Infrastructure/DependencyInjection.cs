using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Papyrus.Application.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.MultiTenancy;
using Papyrus.Infrastructure.Interceptors;

namespace Papyrus.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "papyrus";
    private const string FallbackConnectionString = "Host=localhost;Port=5432;Database=papyrus;Username=postgres;Password=postgres";

    private static readonly Guid SeedProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SeedBlogPostId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SeedCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeedTagDevOpsId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid SeedTagArchitectureId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static IHostApplicationBuilder AddPapyrusInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.Configure<TenantOptions>(builder.Configuration.GetSection(TenantOptions.SectionName));
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<IAuditActor, DefaultAuditActor>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<PapyrusSaveChangesInterceptor>();
        builder.Services.AddScoped<IPublicContentStore, EfPublicContentStore>();
        builder.Services.AddScoped<IContentCommandStore, EfContentCommandStore>();
        builder.Services.AddScoped<IContentQueryStore, EfContentQueryStore>();
        builder.Services.AddScoped<ITaxonomyCommandStore, EfTaxonomyCommandStore>();
        builder.Services.AddScoped<IContentLifecycleStore, EfContentLifecycleStore>();
        builder.Services.AddDbContext<PapyrusDbContext>((serviceProvider, options) =>
        {
            var connectionString = builder.Configuration[$"ConnectionStrings:{ConnectionStringName}"];
            options.UseNpgsql(string.IsNullOrWhiteSpace(connectionString) ? FallbackConnectionString : connectionString, npgsqlOptions =>
            {
                // Tells EF Core to retry up to 5 times if the network stream drops during startup
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
            });
            options.AddInterceptors(serviceProvider.GetRequiredService<PapyrusSaveChangesInterceptor>());
        });
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<PapyrusDbContext>("database");

        return builder;
    }

    // Call only from Papyrus.Api.Admin so migrations run exactly once at startup.
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PapyrusDbContext>();

        await dbContext.Database.MigrateAsync();

        if (string.Equals(app.Environment.ApplicationName, "Papyrus.Api.Admin", StringComparison.Ordinal))
        {
            await SeedAsync(dbContext);
        }
    }

    private static async Task SeedAsync(PapyrusDbContext dbContext)
    {
        if (await dbContext.Projects.AnyAsync())
        {
            return;
        }

        var category = new ContentCategory
        {
            Id = SeedCategoryId,
            TenantId = "main",
            Slug = "platform-engineering",
            Name = "Platform Engineering",
            Description = "Work around delivery pipelines, hosting, infrastructure, and system design.",
            IsPublished = true,
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var devOpsTag = new ContentTag
        {
            Id = SeedTagDevOpsId,
            TenantId = "main",
            Slug = "devops",
            Name = "DevOps",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var architectureTag = new ContentTag
        {
            Id = SeedTagArchitectureId,
            TenantId = "main",
            Slug = "architecture",
            Name = "Architecture",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var project = new Project
        {
            Id = SeedProjectId,
            TenantId = "main",
            CategoryId = SeedCategoryId,
            Slug = "platform-foundation",
            Title = "Papyrus Platform Foundation",
            Summary = "The shared infrastructure for the blog and portfolio modules.",
            Description = "A .NET 10 Aspire-based foundation using PostgreSQL, Entity Framework Core, and separate Razor Pages modules for technical writing and professional showcase content.",
            IsFeatured = true,
            IsPublished = true,
            PublishedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var blogPost = new BlogPost
        {
            Id = SeedBlogPostId,
            TenantId = "main",
            ProjectId = SeedProjectId,
            CategoryId = SeedCategoryId,
            Slug = "building-papyrus-platform-foundation",
            Title = "Building the Papyrus platform foundation",
            Summary = "The first step in shaping a personal site around technical writing and professional showcases.",
            ContentMarkdown = "Papyrus is being shaped as a personal site with two distinct modules: a blog for detailed technical writing and a portfolio for curated professional work. The shared foundation uses ASP.NET Core, Razor Pages, PostgreSQL, and Entity Framework Core.",
            IsFeatured = true,
            IsPublished = true,
            PublishedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var projectTags = new[]
        {
            new ProjectTag { ProjectId = SeedProjectId, TagId = SeedTagDevOpsId, TenantId = "main" },
            new ProjectTag { ProjectId = SeedProjectId, TagId = SeedTagArchitectureId, TenantId = "main" }
        };

        var blogPostTags = new[]
        {
            new BlogPostTag { BlogPostId = SeedBlogPostId, TagId = SeedTagDevOpsId, TenantId = "main" },
            new BlogPostTag { BlogPostId = SeedBlogPostId, TagId = SeedTagArchitectureId, TenantId = "main" }
        };

        dbContext.ContentCategories.Add(category);
        dbContext.ContentTags.AddRange(devOpsTag, architectureTag);
        dbContext.Projects.Add(project);
        dbContext.BlogPosts.Add(blogPost);
        dbContext.ProjectTags.AddRange(projectTags);
        dbContext.BlogPostTags.AddRange(blogPostTags);

        await dbContext.SaveChangesAsync();
    }
}
