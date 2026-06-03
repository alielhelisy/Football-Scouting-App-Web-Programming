using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Helpers;
using ScoutingAppMvc.Models;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
var useSqlite = string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);
var sqlitePath = string.Empty;

if (useSqlite)
{
    sqlitePath = builder.Configuration["SqliteDatabasePath"];
    if (string.IsNullOrWhiteSpace(sqlitePath))
    {
        var homePath = Environment.GetEnvironmentVariable("HOME");
        var dataDir = string.IsNullOrWhiteSpace(homePath)
            ? AppContext.BaseDirectory
            : Path.Combine(homePath, "data");

        Directory.CreateDirectory(dataDir);
        sqlitePath = Path.Combine(dataDir, "scoutingapp.db");
    }
    else
    {
        var sqliteDir = Path.GetDirectoryName(sqlitePath);
        if (!string.IsNullOrWhiteSpace(sqliteDir))
        {
            Directory.CreateDirectory(sqliteDir);
        }
    }

    var shouldResetFromBundle = string.Equals(
        builder.Configuration["SqliteResetFromBundle"],
        "true",
        StringComparison.OrdinalIgnoreCase);
    var bundledDatabasePath = Path.Combine(builder.Environment.ContentRootPath, "scoutingapp-import.db");

    if (shouldResetFromBundle && File.Exists(bundledDatabasePath))
    {
        File.Copy(bundledDatabasePath, sqlitePath, overwrite: true);
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
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

static string RepairMojibake(string value)
{
    if (string.IsNullOrEmpty(value) || (!value.Contains('Ã') && !value.Contains('Â')))
    {
        return value;
    }

    return Encoding.UTF8.GetString(Encoding.Latin1.GetBytes(value));
}

static bool RepairTextEncoding(AppDbContext db)
{
    var changed = false;

    foreach (var player in db.Players)
    {
        var name = RepairMojibake(player.Name);
        var team = RepairMojibake(player.Team);
        var position = RepairMojibake(player.Position);
        var otherPosition = RepairMojibake(player.OtherPosition);
        var firstName = RepairMojibake(player.FirstName);
        var lastName = RepairMojibake(player.LastName);
        var nationality = RepairMojibake(player.Nationality);
        var foot = RepairMojibake(player.Foot);

        if (name != player.Name) { player.Name = name; changed = true; }
        if (team != player.Team) { player.Team = team; changed = true; }
        if (position != player.Position) { player.Position = position; changed = true; }
        if (otherPosition != player.OtherPosition) { player.OtherPosition = otherPosition; changed = true; }
        if (firstName != player.FirstName) { player.FirstName = firstName; changed = true; }
        if (lastName != player.LastName) { player.LastName = lastName; changed = true; }
        if (nationality != player.Nationality) { player.Nationality = nationality; changed = true; }
        if (foot != player.Foot) { player.Foot = foot; changed = true; }
    }

    foreach (var report in db.Reports)
    {
        var ratedPosition = RepairMojibake(report.RatedPosition);
        var receivedCards = RepairMojibake(report.ReceivedCards);
        var comments = report.Comments is null ? null : RepairMojibake(report.Comments);

        if (ratedPosition != report.RatedPosition) { report.RatedPosition = ratedPosition; changed = true; }
        if (receivedCards != report.ReceivedCards) { report.ReceivedCards = receivedCards; changed = true; }
        if (comments != report.Comments) { report.Comments = comments; changed = true; }
    }

    if (changed)
    {
        db.SaveChanges();
    }

    return changed;
}

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

        if (useSqlite && string.Equals(
                builder.Configuration["SqliteRepairTextEncoding"],
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            var repaired = RepairTextEncoding(db);
            Console.WriteLine($"SQLite text encoding repair ran. Changed data: {repaired}");
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
