using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Models;

namespace ScoutingAppMvc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Username).HasColumnName("username");
            e.Property(u => u.Name).HasColumnName("name");
            e.Property(u => u.Surname).HasColumnName("surname");
            e.Property(u => u.Email).HasColumnName("email");
            e.Property(u => u.PasswordHash).HasColumnName("password_hash");
            e.Property(u => u.Role).HasColumnName("role");
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.ToTable("players");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.Name).HasColumnName("name");
            e.Property(p => p.Gender).HasColumnName("gender");
            e.Property(p => p.Team).HasColumnName("team");
            e.Property(p => p.Position).HasColumnName("position");
            e.Property(p => p.OtherPosition).HasColumnName("other_position");
            e.Property(p => p.FirstName).HasColumnName("first_name");
            e.Property(p => p.LastName).HasColumnName("last_name");
            e.Property(p => p.Nationality).HasColumnName("nationality");
            e.Property(p => p.Birthday).HasColumnName("birthday").HasColumnType("date");
            e.Property(p => p.Height).HasColumnName("height");
            e.Property(p => p.Weight).HasColumnName("weight");
            e.Property(p => p.Foot).HasColumnName("foot");
            e.HasOne(p => p.User).WithMany(u => u.Players)
             .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.ToTable("reports");
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.PlayerId).HasColumnName("player_id");
            e.Property(r => r.Rating).HasColumnName("rating").HasColumnType("decimal(2,1)");
            e.Property(r => r.MinutesPlayed).HasColumnName("minutes_played");
            e.Property(r => r.GoalsScored).HasColumnName("goals_scored");
            e.Property(r => r.ReceivedCards).HasColumnName("received_cards");
            e.Property(r => r.RatedPosition).HasColumnName("rated_position");
            e.Property(r => r.Comments).HasColumnName("comments");
            e.Property(r => r.CreatedAt).HasColumnName("created_at").HasColumnType("datetime");
            e.HasOne(r => r.Player).WithMany(p => p.Reports)
             .HasForeignKey(r => r.PlayerId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
