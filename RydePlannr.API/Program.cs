using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RydePlannr.API.Extensions;
using RydePlannr.API.Filters;
using RydePlannr.API.Middleware;
using RydePlannr.Application.Extensions;
using RydePlannr.Domain.Entities;
using RydePlannr.Infrastructure.Extensions;
using RydePlannr.Infrastructure.Persistence;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Pokretanje RydePlannr API-ja...");

    var builder = WebApplication.CreateBuilder(args);

   builder.Host.UseSerilog((context, services, configuration) =>
       configuration.ReadFrom.Configuration(context.Configuration)
           .WriteTo.Console()
           .WriteTo.File(
               path: "Logs/log-.txt",
               rollingInterval: RollingInterval.Day,
               retainedFileCountLimit: 30,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers(options =>
        options.Filters.Add<FluentValidationFilter>());
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddSwaggerWithJwt();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await SeedAdminUserAsync(dbContext);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<LoggingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplikacija se srušila pri pokretanju.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task SeedAdminUserAsync(ApplicationDbContext dbContext)
{
    const string adminEmail = "admin@ride-planner.com";

    if (await dbContext.Users.AnyAsync(u => u.Email == adminEmail))
        return;

    dbContext.Users.Add(new User
    {
        Username = "admin",
        Email = adminEmail,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1!"),
        RoleId = RoleIds.Admin
    });

    await dbContext.SaveChangesAsync();
}