using FCG.Api.Users.Domain.Entities;
using FCG.Api.Users.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCG.Api.Users.Infrastructure.Data.Seed;

public class DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<DatabaseSeeder> _logger = logger;

    public async Task SeedAdminAsync(User adminUser, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == adminUser.Email, cancellationToken);

            if (existingUser != null)
                return;

            await _context.Users.AddAsync(adminUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Admin user {Email} added to database with ID: {UserId}", 
                adminUser.Email, adminUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure admin user exists in database: {Email}", adminUser.Email);
            throw;
        }
    }
}
