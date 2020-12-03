using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public class CredentialService : ICredentialService
    {
        private readonly IRepository<AssetEntity> _assetRepository;
        private readonly IRepository<CredentialEntity> _credentialRepository;
        private readonly IRepository<CredentialValueEntity> _credentialValueRepository;
        private readonly IEmailSender _emailSender;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger<CredentialService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHackerOneService _hackerOneService;

        public CredentialService(IDatabaseLayer databaseLayer, IEmailSender emailSender, ILogger<CredentialService> logger, IConfiguration configuration, IHackerOneService hackerOneService)
        {
            _assetRepository = databaseLayer.Repo<AssetEntity>();
            _credentialValueRepository = databaseLayer.Repo<CredentialValueEntity>();
            _credentialRepository = databaseLayer.Repo<CredentialEntity>();
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
            _hackerOneService = hackerOneService;
        }
        public async Task<IEnumerable<UserCredentialViewModel>> GetCredentials(string hackerName)
        {
            var assets = await GetAssetsForHacker(hackerName);
            return await GetHackerCredentials(hackerName, assets);
        }

        public async Task<IEnumerable<UserCredentialViewModel>> GetCredentialsByAdmin(string hackerName)
        {
            var assets = await _assetRepository.GetAll();
            return await GetHackerCredentials(hackerName, assets);
        }

        private async Task<List<UserCredentialViewModel>> GetHackerCredentials(string hackerName, IEnumerable<AssetEntity> assets)
        {
            var userCredentialsViewModel = new List<UserCredentialViewModel>();
            var credentials = await(
                from credential in _credentialRepository.DbSet.Where(c => c.HackerName == hackerName)
                from credentialValue in _credentialValueRepository.DbSet.Where(cv => cv.AssetName == credential.AssetName && cv.Key == credential.Key)
                select credentialValue
            ).ToListAsync();

            foreach (var asset in assets)
            {
                var userCredentialViewModel = new UserCredentialViewModel
                {
                    AssetName = asset.Key,
                    Description = asset.Description,
                    Columns = asset.Columns.Split(",").ToList(),
                    Credentials = new List<CredentialsViewModel>()
                };
                var sets = credentials.Where(c => c.AssetName == asset.Key).GroupBy(c => c.Key);
                foreach (var credentialEntity in sets)
                {
                    var credentialsViewModel = new CredentialsViewModel
                    {
                        Rows = new Dictionary<int, List<(string ColumnName, string ColumnValue)>>()
                    };

                    foreach (var row in credentialEntity)
                    {
                        if (!credentialsViewModel.Rows.ContainsKey(row.RowNumber))
                            credentialsViewModel.Rows.Add(row.RowNumber, new List<(string ColumnName, string ColumnValue)>());
                        credentialsViewModel.Rows[row.RowNumber].Add((row.ColumnName, row.ColumnValue));
                    }
                    userCredentialViewModel.Credentials.Add(credentialsViewModel);
                }
                userCredentialsViewModel.Add(userCredentialViewModel);
            }
            return userCredentialsViewModel;
        }

        public async Task<string> RequestCredentials(IEnumerable<string> assets, string hackerName, string hackerEmail)
        {
            var errorText = new List<string>();
            foreach (var assetName in assets)
            {
                errorText.Add(await ProcessAsset(assetName, hackerName, hackerEmail));
            }
            return string.Join(". ", errorText);
        }


        private async Task<IEnumerable<AssetEntity>> GetAssetsForHacker(string hackerName, string assetName = "")
        {
            var isHackerInPrivateProgram = await _hackerOneService.IsHackerInPrivateProgram(hackerName);
            var assets = await _assetRepository.FindAll(x => x.IsVisible && x.IsOnHackerOne && (x.IsOnPublicProgram || isHackerInPrivateProgram)
            && (string.IsNullOrWhiteSpace(assetName) || x.Key == assetName));
            return assets;
        }
        
        private async Task<string> ProcessAsset(string assetName, string hackerName, string hackerEmail)
        {
            var asset = (await GetAssetsForHacker(hackerName, assetName)).FirstOrDefault();
            if (asset == null)
                return $"Asset {assetName} not found.";

            var userCredentials = _credentialRepository.DbSet.Count(c => c.AssetName == assetName && c.HackerName == hackerName);
            if (userCredentials >= 2)
                return $"You already have 2 sets of credentials for {assetName}. If you need more, please contact us.";
            IEnumerable<CredentialValueEntity> credentialsData;

            await _semaphore.WaitAsync();
            try
            {
                var credentials = await _credentialRepository.FindOne(c => c.AssetName == assetName && string.IsNullOrEmpty(c.HackerName));
                if (credentials == null)
                    return $"No credentials found for {assetName}. Please contact us to add more.";
                credentialsData = await _credentialValueRepository.FindAll(x => x.AssetName == assetName && x.Key == credentials.Key);
                if (credentialsData == null)
                    return $"No credentials found for {assetName}. Please contact us to add more.";
                credentials.HackerName = hackerName;
                await _credentialRepository.Update(credentials);
            }
            finally
            {
                _semaphore.Release();
            }

            try
            {
                if (SendHackerEmail())
                    SendEmailWithCredentials(asset, hackerEmail, credentialsData.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending email");
                return "An error occured on sending email. Please contact us.";
            }
            try
            {
                SendNotifications(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending notifications");
            }
            return string.Empty;
        }

        public bool SendHackerEmail()
        {
            return bool.TryParse(_configuration["SendHackerEmail"], out var sendEmail) && sendEmail;
        }

        private void SendEmailWithCredentials(AssetEntity asset, string hackerEmail, List<CredentialValueEntity> credentialsData)
        {
            var bodyTemplate = File.ReadAllText("data\\CredentialEmailTemplate.html");
            var header = "<thead><tr>#HEADER</tr></thead>";
            var body = new StringBuilder("");
            var headerData = new StringBuilder();

            asset.Columns.Split(",").ToList().ForEach(column => headerData.Append($"<th>{column}</th>"));
            body.AppendLine(header.Replace("#HEADER", headerData.ToString()));

            foreach (var credentialValueEntities in credentialsData.GroupBy(x => x.RowNumber))
            {
                var lineData = string.Join("",
                    asset.Columns.Split(",")
                        .Select(column => $"<td>{credentialValueEntities.FirstOrDefault(c => c.ColumnName == column)?.ColumnValue ?? ""}</td>"));
                body.AppendLine($"<tr>{lineData}</tr>");
            }

            var message = new EmailMessage(
                new List<string> { hackerEmail },
                $"Credentials for: {asset.Key} ({asset.Description})",
                bodyTemplate.Replace("#TABLEBODY#", body.ToString()));

            _logger.LogInformation($"Sending email to {hackerEmail} for {asset.Key}");
            _emailSender.SendEmail(message);
            _logger.LogInformation("Email send");
        }

        private void SendNotifications(AssetEntity asset)
        {
            if (!int.TryParse(_configuration["NotificationPercent"], out var notificationPercent))
                notificationPercent = 15;
            if (!int.TryParse(_configuration["NotificationNumber"], out var notificationNumber))
                notificationNumber = 10;
            var totalCount = _credentialRepository.DbSet.Count(x => x.AssetName == asset.Key);
            var freeCount = _credentialRepository.DbSet.Count(x => x.AssetName == asset.Key && string.IsNullOrEmpty(x.HackerName));
            if (freeCount < notificationNumber || totalCount > 0 && 100.0m * freeCount / totalCount < notificationPercent)
                SendNotificationEmail(asset, freeCount, totalCount);
        }

        private void SendNotificationEmail(AssetEntity asset, int freeCount, int totalCount)
        {
            var message = new EmailMessage(
                _configuration["NotificationEmailAddresses"].Split(";").ToList(),
                $"Credentials for: {asset.Key} ({asset.Description}) almost empty",
                $"There are only {freeCount} out of {totalCount} Credential left for this asset. Please import more.");

            _logger.LogInformation($"Sending email to {_configuration["NotificationEmailAddresses"]} for {asset.Key} depleted credentials");
            _emailSender.SendEmail(message);
            _logger.LogInformation("Email send");
        }
    }
}