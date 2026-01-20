using FCG.Api.Users;
using FCG.Api.Users.Infrastructure.Data.Context;
using FCG.Api.Users.Infrastructure.Data;
using FCG.Api.Users.Infrastructure.AWS;
using FCG.Api.Users.Application;
using FCG.Api.Users.Infrastructure.Data.Seed;
using FCG.Api.Users.Infrastructure.AWS.Seed;
using FCG.Api.Users.Domain.Entities;
using FCG.Lib.Shared.Infrastructure.Middlewares;
using FCG.Lib.Shared.Messaging.Configuration;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// API Layer - Controllers + Auth + Swagger + CORS
builder.Services.AddApiServices(builder.Configuration);

// FluentValidation Auto Validation
builder.Services.AddFluentValidationAutoValidation();

// Application Layer - CQRS + Validation
builder.Services.AddApplicationServices();

// Infrastructure - Database and Repositories
builder.Services.AddDatabaseInfrastructure(builder.Configuration);

// Infrastructure - AWS
builder.Services.AddAwsInfrastructure(builder.Configuration);

// Messaging - Publisher only
builder.Services.AddMessagingPublisher(builder.Configuration);

var app = builder.Build();

// Database Migration and Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    
    await context.Database.MigrateAsync();

    // Seed admin user from environment variables
    var adminEmail = builder.Configuration["Admin:Email"];
    var adminPassword = builder.Configuration["Admin:Password"];
    var adminName = "Administrator";

    ArgumentException.ThrowIfNullOrEmpty(adminEmail, "Admin email is required for seeding");
    ArgumentException.ThrowIfNullOrEmpty(adminPassword, "Admin password is required for seeding");

    // Create admin user entity
    var adminUser = User.CreateAdmin(adminName, adminEmail);
    
    // Seed Cognito (groups and admin user)
    var cognitoSeeder = services.GetRequiredService<CognitoSeeder>();
    var accountId = await cognitoSeeder.SeedAdminAsync(adminUser, adminPassword);

    adminUser.SetAccountId(accountId);
    
    // Seed Database (admin user)
    var databaseSeeder = services.GetRequiredService<DatabaseSeeder>();
    await databaseSeeder.SeedAdminAsync(adminUser);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Users API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "users-api" }));

app.Run();
