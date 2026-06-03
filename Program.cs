using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Helpers;
using ScoutingAppMvc.Models;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
var useSqlite = string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
        var sqlitePath = builder.Configuration["SqliteDatabasePath"];
        if (string.IsNullOrWhiteSpace(sqlitePath))
        {
            var homePath = Environment.GetEnvironmentVariable("HOME");
            var dataDir = string.IsNullOrWhiteSpace(homePath)
                ? AppContext.BaseDirectory
                : Path.Combine(homePath, "data");

            Directory.CreateDirectory(dataDir);
            sqlitePath = Path.Combine(dataDir, "scoutingapp.db");
        }

        options.UseSqlite($"Data Source={sqlitePath}");
    }
    else
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null));
    }
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.Cookie.Name = "ScoutingAuth";
    });

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (useSqlite)
        {
            db.Database.EnsureCreated();
        }
        else
        {
            // Apply any pending migrations (creates the DB if it doesn't exist)
            db.Database.Migrate();
        }

        // Seed the default admin account if it doesn't exist
        if (!db.Users.Any(u => u.Username == "admin"))
        {
            db.Users.Add(new User
            {
                Username = "admin",
                Name     = "Admin",
                Surname  = "User",
                Email    = "admin@scouting.com",
                PasswordHash = HashHelper.Hash("admin123"),
                Role     = "ADMIN"
            });
            db.SaveChanges();
        }
    }
    catch (Exception ex) when (!app.Environment.IsDevelopment())
    {
        Console.WriteLine($"Database startup check skipped: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/Login");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures      = new[] { new CultureInfo("en-US") },
    SupportedUICultures    = new[] { new CultureInfo("en-US") }
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
