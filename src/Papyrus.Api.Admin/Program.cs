using Papyrus.Api.Admin;
using Papyrus.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(_ =>
        new WeatherForecast
        (Random.Shared.Next(-20, 55)
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

namespace Papyrus.Api.Admin
{
    internal record WeatherForecast(int TemperatureC)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
