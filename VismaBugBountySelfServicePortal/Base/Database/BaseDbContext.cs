using Microsoft.EntityFrameworkCore;

namespace VismaBugBountySelfServicePortal.Base.Database
{
    public class BaseDbContext : DbContext
    {
        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}