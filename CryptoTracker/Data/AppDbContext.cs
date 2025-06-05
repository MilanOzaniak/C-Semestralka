using CryptoTracker.Model;
using Microsoft.EntityFrameworkCore;

namespace CryptoTracker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Coin> Coins { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = "server=localhost;port=3306;database=crypto_db;user=root;password=;";
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}
