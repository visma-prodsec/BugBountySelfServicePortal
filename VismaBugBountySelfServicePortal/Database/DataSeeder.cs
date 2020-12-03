using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Database
{
    public class DataSeeder<TDst> : IDataSeeder where TDst : CommonDataSeedType, new()
    {
        private readonly IDatabaseLayer _databaseLayer;
        
        // ReSharper disable once NotAccessedField.Local
        private readonly ILogger _logger;

        private string FileName => $"data/data-seed{_configuration["SeedEnvironment"]}.json";
        private readonly IConfiguration _configuration;
        private TDst SeedingData { get; set; }

        public DataSeeder(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _databaseLayer = serviceProvider.GetService<IDatabaseLayer>();
            _logger = loggerFactory.CreateLogger<DataSeeder<TDst>>();
            _configuration = configuration;
        }

        private async Task LoadSeedData()
        {
            if (!File.Exists(FileName))
            {
                throw new Exception("Data seed file does not exist.");
            }

            using var stream = new StreamReader(new FileStream(FileName, FileMode.Open, FileAccess.Read));
            SeedingData = JsonConvert.DeserializeObject<TDst>(await stream.ReadToEndAsync());
        }

        private async Task SeedUsers()
        {
            if (SeedingData.Users == null)
            {
                return;
            }

            var repo = _databaseLayer.Repo<UserEntity>();

            var existingEntities = (await repo.GetAll()).ToList();
            foreach (var entity in SeedingData.Users)
            {
                if (existingEntities.Any(s => s.Key == entity.Key))
                    continue;
                await repo.Add(entity);
            }

            await _databaseLayer.SaveChangesAsync();
        }

        private async Task SeedAssets()
        {
            if (SeedingData.Assets == null)
            {
                return;
            }

            var repo = _databaseLayer.Repo<AssetEntity>();

            var existingEntities = (await repo.GetAll()).ToList();
            foreach (var entity in SeedingData.Assets)
            {
                if (existingEntities.Any(s => s.Key == entity.Key))
                    continue;
                await repo.Add(entity);
            }

            await _databaseLayer.SaveChangesAsync();
        }

        private async Task SeedCredentials()
        {
            if (SeedingData.Credentials == null)
            {
                return;
            }

            var repo = _databaseLayer.Repo<CredentialEntity>();

            var existingEntities = (await repo.GetAll()).ToList();
            foreach (var entity in SeedingData.Credentials)
            {
                if (existingEntities.Any(s => s.Key == entity.Key && s.AssetName == entity.AssetName))
                    continue;
                await repo.Add(entity);
            }

            await _databaseLayer.SaveChangesAsync();
        }

        private async Task SeedCredentialValues()
        {
            if (SeedingData.CredentialValues == null)
            {
                return;
            }

            var repo = _databaseLayer.Repo<CredentialValueEntity>();

            var existingEntities = (await repo.GetAll()).ToList();
            foreach (var entity in SeedingData.CredentialValues)
            {
                if (existingEntities.Any(s => s.Key == entity.Key && s.AssetName == entity.AssetName && s.RowNumber == entity.RowNumber && s.ColumnName == entity.ColumnName))
                    continue;
                await repo.Add(entity);
            }

            await _databaseLayer.SaveChangesAsync();
        }

        public async Task MigrateDatabase()
        {
            await _databaseLayer.MigrateDatabase();
        }

        public virtual async Task LoadSeed()
        {
            _logger.LogInformation("Start seeding...");
            await LoadSeedData();
            await SeedUsers();
            await SeedAssets();
            await SeedCredentials();
            await SeedCredentialValues();
            _logger.LogInformation("End seeding...");
        }

        public async Task EnsureMigrated()
        {
            await _databaseLayer.EnsureMigrated();
        }
    }

    public class CommonDataSeedType
    {
        public IEnumerable<UserEntity> Users { get; set; }
        public IEnumerable<AssetEntity> Assets { get; set; }
        public IEnumerable<CredentialEntity> Credentials { get; set; }
        public IEnumerable<CredentialValueEntity> CredentialValues { get; set; }
    }
}
