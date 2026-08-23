using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Papyrus.Infrastructure;

public sealed class PapyrusDbContextFactory : IDesignTimeDbContextFactory<PapyrusDbContext>
{
    private const string DesignTimeConnectionString = "Host=localhost;Port=5432;Database=papyrus;Username=postgres;Password=postgres";

    public PapyrusDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PapyrusDbContext>();
        builder.UseNpgsql(DesignTimeConnectionString);
        return new PapyrusDbContext(builder.Options);
    }
}
