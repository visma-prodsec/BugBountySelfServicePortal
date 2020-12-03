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
            modelBuilder.Entity<CredentialEntity>().ToTable("Credential").HasKey(c=>new {c.Key, c.AssetName});
            modelBuilder.Entity<CredentialValueEntity>().ToTable("CredentialValue").HasKey(c => new { c.AssetName, c.Key,  c.RowNumber, c.ColumnName });
            modelBuilder.Entity<UserEntity>().ToTable("User");
            modelBuilder.Entity<UserSessionEntity>().ToTable("UserSession");
            base.OnModelCreating(modelBuilder);
        }
    }
}
