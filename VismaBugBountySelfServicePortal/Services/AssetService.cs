using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Models.Entity;
using VismaBugBountySelfServicePortal.Models.ViewModel;

namespace VismaBugBountySelfServicePortal.Services
{
    public class AssetService : IAssetService
    {
        private const int TenMB = 10 * 1024 * 1024;
        private readonly ILogger<AssetService> _logger;
        private readonly IRepository<AssetEntity> _assetRepository;
        private readonly IRepository<CredentialEntity> _credentialRepository;
        private readonly IRepository<CredentialValueEntity> _credentialValueRepository;
        private readonly IConfiguration _configuration;
        private readonly IHackerOneService _hackerOneService;
        private string SetIdColumn => _configuration["SetIdColumn"];
        private string HackerNameColumn => _configuration["HackerNameColumn"];

        public AssetService(IDatabaseLayer databaseLayer, IConfiguration configuration, ILogger<AssetService> logger, IHackerOneService hackerOneService)
        {
            _assetRepository = databaseLayer.Repo<AssetEntity>();
            _credentialValueRepository = databaseLayer.Repo<CredentialValueEntity>();
            _credentialRepository = databaseLayer.Repo<CredentialEntity>();
            _configuration = configuration;
            _logger = logger;
            _hackerOneService = hackerOneService;
        }

        public async Task<IEnumerable<AssetViewModel>> GetAssets()
        {
            var assets = await (
                from asset in _assetRepository.DbSet
                join allCredentials in (from credentialsAll in _credentialRepository.DbSet.GroupBy(x => x.AssetName)
                                        select new { credentialsAll.Key, Cnt = credentialsAll.Count() }) on asset.Key equals allCredentials.Key into allCred
                from p in allCred.DefaultIfEmpty()
                join freeCredentials in (from credentialsAll in _credentialRepository.DbSet.Where(x => string.IsNullOrWhiteSpace(x.HackerName)).GroupBy(x => x.AssetName)
                                         select new { credentialsAll.Key, Cnt = credentialsAll.Count() }) on asset.Key equals freeCredentials.Key into freeCred
                from f in freeCred.DefaultIfEmpty()
                select new AssetViewModel
                {
                    Name = asset.Key,
                    Description = asset.Description,
                    IsVisible = asset.IsVisible,
                    IsOnHackerOne = asset.IsOnHackerOne,
                    IsOnPublicProgram = asset.IsOnPublicProgram,
                    Free = f.Cnt,
                    Total = p.Cnt
                }

            ).ToListAsync();
            return assets;
        }

        public async Task<IEnumerable<object>> GetAssetCredentialsForExport(string assetName)
        {
            var credentials = await (
                from asset in _assetRepository.DbSet.Where(x => x.Key == assetName)
                from credential in _credentialRepository.DbSet.Where(c => c.AssetName == assetName)
                from credentialValue in _credentialValueRepository.DbSet.Where(cv => cv.AssetName == credential.AssetName && cv.Key == credential.Key)
                select new
                {
                    asset.Columns,
                    credential.Key,
                    credential.HackerName,
                    credentialValue.ColumnName,
                    credentialValue.ColumnValue,
                    credentialValue.RowNumber
                }
            ).ToListAsync();
            var group = credentials.GroupBy(x => new { x.Key, x.HackerName, x.Columns, x.RowNumber });
            var rows = new List<ExpandoObject>();
            foreach (var item in group.OrderBy(x => int.TryParse(x.Key.Key, out var z) ? z : 0))
            {
                var line = new ExpandoObject() as IDictionary<string, object>;
                var columns = item.Key.Columns.Split(",").ToList();
                line.Add(SetIdColumn, item.Key.Key);
                foreach (var column in columns)
                {
                    var value = item.FirstOrDefault(x => x.ColumnName == column);
                    line.Add(column, value?.ColumnValue);
                }
                line.Add(HackerNameColumn, item.Key.HackerName);
                rows.Add((ExpandoObject)line);
            }

            return rows;
        }

        public async Task DeleteAsset(string deleteAssetId)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                await _credentialValueRepository.DbSet.Where(x => x.AssetName == deleteAssetId).BatchDeleteAsync();
                await _credentialRepository.DbSet.Where(x => x.AssetName == deleteAssetId).BatchDeleteAsync();
                await _assetRepository.Delete(deleteAssetId);
                scope.Complete();
            }
            // ReSharper disable once RedundantEmptyFinallyBlock
            finally
            {

            }
        }

        public async Task<UserCredentialViewModel> GetAssetCredentials(string selectedAsset)
        {
            var asset = await _assetRepository.FindOne(x => x.Key == selectedAsset);
            if (asset == null)
                return null;
            var credentials = await (
                        from credential in _credentialRepository.DbSet.Where(c => c.AssetName == asset.Key).OrderByDescending(s => s.HackerName)
                        from credentialValue in _credentialValueRepository.DbSet.Where(cv => cv.AssetName == credential.AssetName && cv.Key == credential.Key)
                        select new { Credential = credential, CredentialValue = credentialValue }
                    ).ToListAsync();
            var userCredentialViewModel = new UserCredentialViewModel
            {
                AssetName = asset.Key,
                Description = asset.Description,
                Columns = asset.Columns.Split(",").ToList(),
                Credentials = new List<CredentialsViewModel>(),
            };
            foreach (var credentialEntity in credentials.GroupBy(c => c.Credential))
            {
                var credentialsViewModel = new CredentialsViewModel
                {
                    Rows = new Dictionary<int, List<(string ColumnName, string ColumnValue)>>(),
                    HackerName = credentialEntity.Key.HackerName
                };
                foreach (var row in credentialEntity.Select(x => x.CredentialValue))
                {
                    if (!credentialsViewModel.Rows.ContainsKey(row.RowNumber))
                        credentialsViewModel.Rows.Add(row.RowNumber, new List<(string ColumnName, string ColumnValue)>());
                    credentialsViewModel.Rows[row.RowNumber].Add((row.ColumnName, row.ColumnValue));
                }

                userCredentialViewModel.Credentials.Add(credentialsViewModel);
            }

            return userCredentialViewModel;
        }

        public async Task<AddEditAssetViewModel> GetAssetDetails(string selectedAsset)
        {
            var asset = await _assetRepository.FindOne(x => x.Key == selectedAsset);
            if (asset == null)
                return null;
            return new AddEditAssetViewModel
            {
                Name = asset.Key,
                Description = asset.Description,
                Columns = asset.Columns
            };
        }

        public async Task<(string message, bool? isError)> UpdateAssetVisibility(string assetName, bool isVisible)
        {
            var asset = await _assetRepository.FindOne(x => x.Key == assetName);
            if (asset == null)
                return ($"Asset {assetName} not found.", true);
            asset.IsVisible = isVisible;
            await _assetRepository.Update(asset);
            if (!asset.IsOnHackerOne && asset.IsVisible)
                return ($"Asset {assetName} not found on H1, so the users will not see it. Please sync data so it is visible for users.", false);
            return (string.Empty, null);
        }

        public async Task<string> AddOrUpdate(AddEditAssetViewModel model)
        {
            model.Name = model.Name.Trim();
            model.Description = model.Description.Trim();
            model.Columns = model.Columns.Trim();

            var asset = await _assetRepository.GetOne(model.Name);
            if (asset == null)
            {
                if (model.IsNew)
                    asset = new AssetEntity { Key = model.Name };
                else return "Asset not found.";
            }
            else if (model.IsNew)
                return "Asset already exists.";

            var columns = model.Columns.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            if (columns.Count < 2)
                return "There should be at least 2 columns separated by comma.";

            asset.Description = model.Description;
            asset.Columns = string.Join(",", columns);

            await _assetRepository.AddOrUpdate(asset);
            return string.Empty;
        }

        public async Task<(string, bool)> ImportFile(string assetName, IFormFile file)
        {
            if (file == null)
                return ("Data file should not be empty.", true);
            if (file.Length > TenMB)
                return ("File too big.", true);
            var asset = await _assetRepository.GetOne(assetName);
            if (asset == null)
                return ($"Asset {assetName} not found.", true);
            var columns = asset.Columns.Split(",").ToList();
            try
            {
                var credentials = await ParseCsv(assetName, file, columns);
                return (await AddCredentialData(assetName, credentials), false);
            }
            catch (ArgumentException argEx)
            {
                return (argEx.Message, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on importing csv");
                return (ex.Message, true);
            }
        }

        public async Task SyncAssets()
        {
            var privateHackerOneAssets = await _hackerOneService.GetAssets(true);
            var publicHackerOneAssets = await _hackerOneService.GetAssets(false);
            var assets = await _assetRepository.GetAll();
            foreach (var asset in assets)
            {
                var assetList = asset.Key.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                var update = false;
                var privateAsset = privateHackerOneAssets.FirstOrDefault(x => assetList.Contains(x.Trim()));
                var publicAsset = publicHackerOneAssets.FirstOrDefault(x => assetList.Contains(x.Trim()));
                if (!string.IsNullOrWhiteSpace(privateAsset))
                {
                    if (!asset.IsOnHackerOne)
                    {
                        asset.IsOnHackerOne = true;
                        update = true;
                    }

                    if (asset.IsOnPublicProgram)
                    {
                        asset.IsOnPublicProgram = false;
                        update = true;
                    }
                }
                else
                if (!string.IsNullOrWhiteSpace(publicAsset))
                {
                    if (!asset.IsOnHackerOne)
                    {
                        asset.IsOnHackerOne = true;
                        update = true;
                    }
                    if (!asset.IsOnPublicProgram)
                    {
                        asset.IsOnPublicProgram = true;
                        update = true;
                    }
                }
                else if (asset.IsOnHackerOne)
                {
                    asset.IsOnHackerOne = false;
                    update = true;
                }

                if (!asset.IsOnHackerOne && asset.IsOnPublicProgram)
                {
                    asset.IsOnPublicProgram = false;
                    update = true;
                }
                if (update)
                    await _assetRepository.Update(asset);
            }
        }

        private async Task<string> AddCredentialData(string assetName, List<CredentialEntity> credentials)
        {
            var added = 0;
            var updated = 0;
            var skipped = 0;
            var existingCredentials = new Dictionary<string, CredentialEntity>();
            (await _credentialRepository.FindAll(x => x.AssetName == assetName)).ToList().ForEach(c => existingCredentials.Add(c.Key, c));
            (await _credentialValueRepository.FindAll(x => x.AssetName == assetName)).ToList().ForEach(cv =>
              {
                  existingCredentials[cv.Key].Rows.Add(cv);
              });
            var updateCredentials = new List<CredentialEntity>();
            var insertCredentialValues = new List<CredentialValueEntity>();
            var deleteCredentialValues = new List<CredentialValueEntity>();
            foreach (var credential in credentials)
            {
                var setId = credential.Key;
                if (existingCredentials.ContainsKey(setId))
                {
                    //skip existing with hacker name set
                    if (!string.IsNullOrWhiteSpace(existingCredentials[setId].HackerName))
                    {
                        skipped++;
                        continue;
                    }

                    if (existingCredentials[setId].HackerName != credential.HackerName)
                    {
                        existingCredentials[setId].HackerName = credential.HackerName;
                        updateCredentials.Add(existingCredentials[setId]);
                        updated++;
                    }

                    if (AreSame(credential.Rows, existingCredentials[setId].Rows))
                    {
                        skipped++;
                        continue;
                    }

                    insertCredentialValues.AddRange(credential.Rows);
                    deleteCredentialValues.AddRange(existingCredentials[setId].Rows);
                }
                else
                {
                    added++;
                    updateCredentials.Add(credential);
                    insertCredentialValues.AddRange(credential.Rows);
                }

            }

            await _credentialRepository.AddOrUpdateMultiple(updateCredentials);
            foreach (var credentialValue in deleteCredentialValues)
                await _credentialValueRepository.Delete(credentialValue);
            await _credentialValueRepository.AddOrUpdateMultiple(insertCredentialValues);
            return $"{added} added, {updated} updated, {skipped} skipped.";
        }

        private async Task<List<CredentialEntity>> ParseCsv(string assetName, IFormFile file, List<string> columns)
        {
            var mapping = new Dictionary<string, int>();

            var credentialDict = new Dictionary<string, CredentialEntity>();

            columns.Add(SetIdColumn);
            columns.Add(HackerNameColumn);
            columns.ForEach(c => mapping.Add(c, -1));
            var rowNumbers = new Dictionary<string, int>();

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            var firstLine = true;
            var header = new string[] { };
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    continue;
                if (firstLine)
                {
                    header = line.GetCsvElements().ToArray();
                    for (var i = 0; i < header.Length; i++)
                    {
                        if (mapping.ContainsKey(header[i]))
                            mapping[header[i]] = i;
                    }

                    if (mapping.ContainsValue(-1))
                        throw new ArgumentException($"Invalid header. Missing columns: {string.Join(",", mapping.Where(x => x.Value == -1).Select(x => x.Key))}");

                    firstLine = false;
                    continue;
                }

                var lineData = line.GetCsvElements().ToArray();
                if (lineData.Length != header.Length)
                    throw new ArgumentException($"Invalid line: {line}");

                var setId = lineData[mapping[SetIdColumn]];
                var hackerName = lineData[mapping[HackerNameColumn]];

                if (!credentialDict.ContainsKey(setId))
                {
                    credentialDict[setId] = new CredentialEntity { AssetName = assetName, Key = setId, HackerName = hackerName };
                    rowNumbers[setId] = 0;
                }

                var credential = credentialDict[setId];
                if (credential.HackerName != hackerName)
                    throw new ArgumentException($"SetId: {setId} has different hacker name on rows, set hacker name:{credential.HackerName}, line hacker name: {hackerName}");



                rowNumbers[setId]++;

                credential.Rows.AddRange(mapping.Where(map => map.Key != SetIdColumn && map.Key != HackerNameColumn)
                    .Select(map => new CredentialValueEntity
                    {
                        AssetName = assetName,
                        ColumnName = map.Key,
                        ColumnValue = lineData[map.Value],
                        RowNumber = rowNumbers[setId],
                        Key = setId
                    }));

            }

            return credentialDict.Values.ToList();
        }

        private static bool AreSame(IReadOnlyCollection<CredentialValueEntity> rowA, List<CredentialValueEntity> rowB)
        {
            return rowA.Count == rowB.Count
                   && rowA.All(row =>
                       rowB.Exists(r => r.AssetName == row.AssetName && r.ColumnName == row.ColumnName && r.Key == row.Key && r.ColumnValue == row.ColumnValue));
        }
    }
}