# Papyrus

Papyrus is a .NET 10 modular monolith organized around vertical slices. The public Razor Pages modules and the centralized admin API share Core contracts and Infrastructure, while Aspire owns local orchestration.

## Create From Scratch

Run these commands from an empty directory. The Aspire template package is installed once per machine.

```powershell
dotnet new install Aspire.ProjectTemplates
dotnet new sln -n Papyrus --format sln

dotnet new aspire-apphost -n Papyrus.AppHost -o src/Papyrus.AppHost -f net10.0
dotnet new aspire-servicedefaults -n Papyrus.ServiceDefaults -o src/Papyrus.ServiceDefaults -f net10.0
dotnet new classlib -n Papyrus.Core -o src/Papyrus.Core -f net10.0
dotnet new classlib -n Papyrus.Infrastructure -o src/Papyrus.Infrastructure -f net10.0
dotnet new webapp -n Papyrus.Web.Portfolio -o src/Papyrus.Web.Portfolio -f net10.0
dotnet new webapp -n Papyrus.Web.Blog -o src/Papyrus.Web.Blog -f net10.0
dotnet new webapi -n Papyrus.Api.Admin -o src/Papyrus.Api.Admin -f net10.0

dotnet sln Papyrus.sln add src/Papyrus.AppHost/Papyrus.AppHost.csproj src/Papyrus.ServiceDefaults/Papyrus.ServiceDefaults.csproj src/Papyrus.Core/Papyrus.Core.csproj src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj src/Papyrus.Web.Portfolio/Papyrus.Web.Portfolio.csproj src/Papyrus.Web.Blog/Papyrus.Web.Blog.csproj src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj

dotnet add src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj reference src/Papyrus.Core/Papyrus.Core.csproj
dotnet add src/Papyrus.Web.Portfolio/Papyrus.Web.Portfolio.csproj reference src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj src/Papyrus.Core/Papyrus.Core.csproj src/Papyrus.ServiceDefaults/Papyrus.ServiceDefaults.csproj
dotnet add src/Papyrus.Web.Blog/Papyrus.Web.Blog.csproj reference src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj src/Papyrus.Core/Papyrus.Core.csproj src/Papyrus.ServiceDefaults/Papyrus.ServiceDefaults.csproj
dotnet add src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj reference src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj src/Papyrus.Core/Papyrus.Core.csproj src/Papyrus.ServiceDefaults/Papyrus.ServiceDefaults.csproj
dotnet add src/Papyrus.AppHost/Papyrus.AppHost.csproj reference src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj src/Papyrus.Web.Portfolio/Papyrus.Web.Portfolio.csproj src/Papyrus.Web.Blog/Papyrus.Web.Blog.csproj

dotnet add src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj package Finbuckle.MultiTenant.EntityFrameworkCore
dotnet add src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## Layout

```text
Papyrus.sln
README.md
src/
  Papyrus.AppHost/
    Program.cs
    Papyrus.AppHost.csproj
  Papyrus.ServiceDefaults/
    Extensions.cs
    Papyrus.ServiceDefaults.csproj
  Papyrus.Core/
    MultiTenancy/ITenantEntity.cs
    Papyrus.Core.csproj
  Papyrus.Infrastructure/
    PapyrusDbContext.cs
    Papyrus.Infrastructure.csproj
  Papyrus.Web.Portfolio/
    Pages/
    Program.cs
    Papyrus.Web.Portfolio.csproj
  Papyrus.Web.Blog/
    Pages/
    Program.cs
    Papyrus.Web.Blog.csproj
  Papyrus.Api.Admin/
    Program.cs
    Papyrus.Api.Admin.csproj
```

## Run

```powershell
dotnet build Papyrus.sln
dotnet run --project src/Papyrus.AppHost/Papyrus.AppHost.csproj
```

`Papyrus.Infrastructure/PapyrusDbContext.cs` implements Finbuckle's `IMultiTenantDbContext`, captures the current tenant from `IMultiTenantContextAccessor`, and calls `ConfigureMultiTenant()` during model creation. Add tenant resolution, connection-string selection, and tenant-scoped entities as each vertical slice is implemented.
