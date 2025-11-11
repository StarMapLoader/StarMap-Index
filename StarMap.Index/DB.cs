using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace StarMapIndex
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<Mod> Mods => Set<Mod>();
        public DbSet<ModVersion> Versions => Set<ModVersion>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>().HasIndex(u => u.GithubId).IsUnique();
            mb.Entity<Mod>().HasKey(u => u.Id);

            mb.Entity<Mod>().HasKey(m => m.Id);

            mb.Entity<Mod>()
                .HasOne(m => m.Author)
                .WithMany()
                .HasForeignKey(m => m.AuthorId);

            mb.Entity<Mod>()
               .HasOne(m => m.LatestVersion)
               .WithMany()
               .HasForeignKey(m => m.LatestVersionId)
               .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ModVersion>().HasKey(v => v.Id);
            mb.Entity<ModVersion>().HasIndex(v => v.ModId);
            mb.Entity<ModVersion>()
               .HasOne(v => v.Mod)
               .WithMany()
               .HasForeignKey(v => v.ModId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string? GithubId { get; set; }
        public string? DisplayName { get; set; }
    }

    public class Mod
    {
        public Guid Id { get; set; }

        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public string ApiKeyHash { get; set; } = "";
        // visible identifier for the key (not the secret)
        public string ApiKeyId { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? LatestVersionId { get; set; }
        public ModVersion? LatestVersion { get; set; }
    }

    public class ModVersion
    {
        public Guid Id { get; set; }
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid ModId { get; set; }
        public Mod Mod { get; set; } = null!;
    }

    public class AddVersionRequestBody
    {
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
    }

        public static class CryptoHelpers
    {
        public static string GenerateApiKey(int bytes = 32)
        {
            var b = new byte[bytes];
            RandomNumberGenerator.Fill(b);
            // base64 url-safe (no padding) for convenience
            return Convert.ToBase64String(b);
        }

        public static string HashString(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashed = sha.ComputeHash(bytes);
            return Convert.ToHexString(hashed); // uppercase hex
        }
    }
}
