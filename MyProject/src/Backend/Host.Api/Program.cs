using Host.Api;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvFile();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddCatalogModule(builder.Configuration);

var app = builder.Build();

// OpenAPI spec — available in all environments (contract publishing, CI/CD)
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    // Scalar UI — dev only (attack surface in production)
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    context.Database.EnsureCreated();
}

app.UseExceptionHandler();
app.MapCatalogEndpoints();

app.Run();
