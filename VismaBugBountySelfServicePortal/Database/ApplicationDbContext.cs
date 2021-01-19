using Microsoft.EntityFrameworkCore;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssetEntity>().ToTable("Asset");
            modelBuilder.Entity<CredentialEntity>().ToTable("Credential").HasKey(c => new { c.Key, c.AssetName });
            modelBuilder.Entity<CredentialValueEntity>().ToTable("CredentialValue").HasKey(c => new { c.AssetName, c.Key, c.RowNumber, c.ColumnName });
            modelBuilder.Entity<UserEntity>().ToTable("User");
            modelBuilder.Entity<UserSessionEntity>().ToTable("UserSession").HasKey(s => new { s.Key, s.SessionId });
            modelBuilder.Entity<UserSessionHistoryEntity>().ToTable("UserSessionHistory").HasKey(s => new { s.Key, s.SessionId });
            modelBuilder.Entity<TransferCredentialHistoryEntity>().ToTable("TransferCredentialHistory").HasKey(s => new { s.Key, s.ToEmail });
            modelBuilder.Entity<RequestCredentialHistoryEntity>().ToTable("RequestCredentialHistory").HasKey(c => new { c.Key, c.AssetName, c.HackerName });

            base.OnModelCreating(modelBuilder);
        }
    }
}
