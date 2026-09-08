using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Interceptors;

namespace Papyrus.Tests;

internal static class TestDb
{
    public static PapyrusDbContext Create()
    {
        var options = new DbContextOptionsBuilder<PapyrusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new PapyrusSaveChangesInterceptor(new DefaultAuditActor(), TimeProvider.System))
            .Options;
        return new PapyrusDbContext(options);
    }

    public static PapyrusDbContext CreatePostgres(string connectionString, string tenantId = "main",
        params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<PapyrusDbContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(interceptors)
            .Options;
        return new PapyrusDbContext(options, new Papyrus.Infrastructure.MultiTenancy.TenantContext(tenantId));
    }
}
