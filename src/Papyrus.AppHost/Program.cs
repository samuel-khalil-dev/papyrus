var builder = DistributedApplication.CreateBuilder(args);

var adminApi = builder.AddProject<Projects.Papyrus_Api_Admin>("admin-api");
var portfolio = builder.AddProject<Projects.Papyrus_Web_Portfolio>("portfolio");
var blog = builder.AddProject<Projects.Papyrus_Web_Blog>("blog");

portfolio.WithReference(adminApi);
blog.WithReference(adminApi);

builder.Build().Run();