using Aspire.Hosting.JavaScript;

var builder = DistributedApplication.CreateBuilder(args);

var pgUser = builder.AddParameter("pg-user");
var pgPass = builder.AddParameter("pg-pass", secret: true);

var postgres = builder.AddPostgres("postgres", userName: pgUser, password: pgPass, port: 5432)
    .WithDataVolume();
var papyrusDb = postgres.AddDatabase("papyrus");

// Admin API is the single migrator. Blog and Portfolio wait for it to become healthy
// so they never race Postgres or run migrations themselves.
var adminApi = builder.AddProject<Projects.Papyrus_Api_Admin>("admin-api")
    .WithReference(papyrusDb)
    .WithEnvironment("AdminApi__ApiKey", "dev-admin-key")
    .WithHttpHealthCheck("/health")
    .WaitFor(postgres)
    .WaitFor(papyrusDb);

var adminUi = builder.AddJavaScriptApp("admin-ui", "../Papyrus.Admin.UI", "dev")
    .WithReference(adminApi)
    .WithHttpEndpoint(targetPort: 5173, name: "http")
    .WithEnvironment("VITE_API_BASE_URL", adminApi.GetEndpoint("http"));

var portfolio = builder.AddProject<Projects.Papyrus_Web_Portfolio>("portfolio")
    .WithReference(adminApi)
    .WithReference(papyrusDb)
    .WithHttpHealthCheck("/health")
    .WaitFor(postgres)
    .WaitFor(papyrusDb)
    .WaitFor(adminApi);

var blog = builder.AddProject<Projects.Papyrus_Web_Blog>("blog")
    .WithReference(adminApi)
    .WithReference(papyrusDb)
    .WithHttpHealthCheck("/health")
    .WaitFor(postgres)
    .WaitFor(papyrusDb)
    .WaitFor(adminApi);

builder.Build().Run();
