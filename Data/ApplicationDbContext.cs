using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using server.Models;
using System.Reflection.Emit;

namespace server.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<StockPrice> StockPrices { get; set; }
        public DbSet<Watchlist> Watchlists { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.AppUser)
                    .WithMany(u => u.Portfolios)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Stock)
                    .WithMany(s => s.Portfolios)
                    .HasForeignKey(p => p.StockId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => new { p.UserId, p.StockId });
            });

            // Configure Transaction relationships
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.AppUser)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Stock)
                    .WithMany(s => s.Transactions)
                    .HasForeignKey(t => t.StockId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.UserId, t.TransactionDate });
            });

            // Configure StockPrice relationships
            modelBuilder.Entity<StockPrice>(entity =>
            {
                entity.HasOne(sp => sp.Stock)
                    .WithMany(s => s.StockPrices)
                    .HasForeignKey(sp => sp.StockId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(sp => new { sp.StockId, sp.Date }).IsUnique();
            });

            // Configure Watchlist relationships
            modelBuilder.Entity<Watchlist>(entity =>
            {
                entity.HasOne(w => w.AppUser)
                    .WithMany(u => u.Watchlists)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Stock)
                    .WithMany()
                    .HasForeignKey(w => w.StockId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => new { w.UserId, w.StockId }).IsUnique();
            });

            // Configure Alert relationships
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasOne(a => a.AppUser)
                    .WithMany(u => u.Alerts)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Stock)
                    .WithMany()
                    .HasForeignKey(a => a.StockId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => new { a.UserId, a.IsActive });
            });

            // Configure Comment relationships
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.AppUser)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Stock)
                    .WithMany(s => s.Comments)
                    .HasForeignKey(c => c.StockId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Stock
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasIndex(s => s.Symbol).IsUnique();
                entity.Property(s => s.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(s => s.CompanyName).IsRequired().HasMaxLength(100);
            });

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "User", NormalizedName = "USER" }
            };
            modelBuilder.Entity<IdentityRole>().HasData(roles);
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed some popular stocks
            modelBuilder.Entity<Stock>().HasData(
                new Stock
                {
                    Id = 1,
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Purchase = 150.00m,
                    CurrentPrice = 175.25m,
                    MarketCap = 2800000000000,
                    LastDiv = 0.96m,
                    Industry = "Technology",
                    Country = "US",
                    PERatio = 28.5m,
                    DividendYield = 0.55m,
                    IsActive = true
                },
                new Stock
                {
                    Id = 2,
                    Symbol = "GOOGL",
                    CompanyName = "Alphabet Inc.",
                    Purchase = 2500.00m,
                    CurrentPrice = 2750.50m,
                    MarketCap = 1800000000000,
                    LastDiv = 0.00m,
                    Industry = "Technology",
                    Country = "US",
                    PERatio = 25.2m,
                    DividendYield = 0.00m,
                    IsActive = true
                },
                new Stock
                {
                    Id = 3,
                    Symbol = "MSFT",
                    CompanyName = "Microsoft Corporation",
                    Purchase = 300.00m,
                    CurrentPrice = 325.75m,
                    MarketCap = 2400000000000,
                    LastDiv = 2.72m,
                    Industry = "Technology",
                    Country = "US",
                    PERatio = 27.8m,
                    DividendYield = 0.84m,
                    IsActive = true
                }
            );
        }
    }
}
