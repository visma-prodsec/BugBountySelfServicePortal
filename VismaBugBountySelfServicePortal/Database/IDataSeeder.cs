using System.Threading.Tasks;

namespace VismaBugBountySelfServicePortal.Database
{
    public interface IDataSeeder
    {
        Task MigrateDatabase();
        Task LoadSeed();
        Task EnsureMigrated();
    }
}