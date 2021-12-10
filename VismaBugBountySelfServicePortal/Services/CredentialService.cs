using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Models.Entity;
using VismaBugBountySelfServicePortal.Models.ViewModel;

namespace VismaBugBountySelfServicePortal.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly IEmailSender _emailSender;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<CredentialService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProviderFactory _providerFactory;
        private readonly IDatabaseLayer _databaseLayer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CredentialService(IDatabaseLayer databaseLayer, IEmailSender emailSender, ILogger<CredentialService> logger, IConfiguration configuration, IProviderFactory providerFactory, IHttpContextAccessor httpContextAccessor)
        {
            _databaseLayer = databaseLayer;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
            _providerFactory = providerFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<UserCredentialViewModel>> GetCredentials(string hackerName, string hackerEmail, bool transferred)
        {
            var assets = await GetAssetsForHacker(hackerName, hackerEmail);
            return await GetHackerCredentials(hackerName, assets, transferred);
        }

        public async Task<IEnumerable<UserCredentialViewModel>> GetCredentialsByAdmin(string hackerName)
        {
            if (!_httpContextAccessor?.HttpContext?.User.IsInRole(Const.AdminRole) ?? false)
                return null;
            var assets = await _databaseLayer.Repo<AssetEntity>().GetAll();
            return await GetHackerCredentials(hackerName, assets, null);
        }

        private async Task<List<UserCredentialViewModel>> GetHackerCredentials(string hackerName, IEnumerable<AssetEntity> assets, bool? transferred)
        {
            var userCredentialsViewModel = new List<UserCredentialViewModel>();
            var credentialsRows = await (
                from credential in _databaseLayer.Repo<CredentialEntity>().DbSet.Where(c => c.HackerName == hackerName
                                            && (!transferred.HasValue || (c.Transferred ?? true) == transferred.Value))
                from credentialValue in _databaseLayer.Repo<CredentialValueEntity>().DbSet.Where(cv => cv.AssetName == credential.AssetName && cv.Key == credential.Key)
                select new { credentialValue, credential.Transferred }
            ).ToListAsync();
            var credentialTransferred = credentialsRows.FirstOrDefault()?.Transferred;
            var credentials = credentialsRows.Select(c => c.credentialValue).ToList();
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
                        Rows = new Dictionary<int, List<(string ColumnName, string ColumnValue)>>(),
                        Transferred = credentialTransferred
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


        private async Task<IEnumerable<AssetEntity>> GetAssetsForHacker(string hackerName, string hackerEmail, string assetName = "")
        {
            var providerService = _providerFactory.GetProviderService(hackerEmail.Split('@').Last());
            bool isOurUser = _configuration["SecurityTeamUsers"].Split(',').Any(x => x.Equals(hackerName, StringComparison.InvariantCultureIgnoreCase));
            if (hackerName.Contains("\\") || hackerName.Contains("/") || hackerName.Contains("."))
                return new List<AssetEntity>();
            var hackerPrograms = (await providerService.GetHackerProgramList(hackerName)).Select(p => p.Name);
            var assets = (await _databaseLayer.Repo<AssetEntity>()
                .FindAll(asset => asset.IsVisible && asset.IsOnHackerOne && (string.IsNullOrWhiteSpace(assetName) || asset.Key == assetName))).ToList()
                .Where(asset => asset.IsOnPublicProgram
                                || isOurUser
                                || hackerPrograms.Any(hackerProgram =>
                                    asset.Programs?.Split(Const.DatabaseSeparator).Any(assetProgram => assetProgram.Equals(hackerProgram, StringComparison.InvariantCultureIgnoreCase)) ?? false));
            return assets;
        }

        private async Task<string> ProcessAsset(string assetName, string hackerName, string hackerEmail)
        {
            var credentialRepository = _databaseLayer.Repo<CredentialEntity>();
            var credentialValueRepository = _databaseLayer.Repo<CredentialValueEntity>();
            var asset = (await GetAssetsForHacker(hackerName, hackerEmail, assetName)).FirstOrDefault();
            if (asset == null)
                return $"Asset {assetName} not found.";

            var userCredentials = credentialRepository.DbSet.Count(c => c.AssetName == assetName && c.HackerName == hackerName && (c.Transferred ?? true));
            var limit = (!_httpContextAccessor?.HttpContext?.User.IsInRole(Const.AdminRole) ?? false) ? 2 : 4;
            if (userCredentials >= limit)
                return $"You already have {limit} sets of credentials for {assetName}. If you need more, please contact us.";
            IEnumerable<CredentialValueEntity> credentialsData;

            await _semaphore.WaitAsync();
            try
            {
                var credentials = await credentialRepository.FindOne(c => c.AssetName == assetName && string.IsNullOrEmpty(c.HackerName));
                if (credentials == null)
                    return $"No credentials found for {assetName}. Please contact us to add more.";
                credentialsData = await credentialValueRepository.FindAll(x => x.AssetName == assetName && x.Key == credentials.Key);
                if (credentialsData == null)
                    return $"No credentials found for {assetName}. Please contact us to add more.";
                credentials.HackerName = hackerName;
                await credentialRepository.Update(credentials);
                var requestCredentialHistory = new RequestCredentialHistoryEntity { Key = credentials.Key, AssetName = credentials.AssetName, HackerName = hackerName, RequestDateTime = DateTime.UtcNow };
                await _databaseLayer.Repo<RequestCredentialHistoryEntity>().Add(requestCredentialHistory);
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
                return "An error occurred on sending email. Please contact us.";
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

        public async Task<string> TransferCredentials(string hackerName, string hackerEmail, string newEmail)
        {
            var credentialRepository = _databaseLayer.Repo<CredentialEntity>();
            if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.EndsWith(_configuration["ProviderEmailDomain"]))
                return "Invalid email";
            var newUser = newEmail.Split('@', '-', '+')[0];
            var credentials = await credentialRepository.DbSet.Where(c => c.HackerName == hackerName && c.Transferred.HasValue && c.Transferred.Value == false).ToListAsync();
            if (credentials.Count == 0)
                return "No credential found to transfer. In case you think this is a mistake, please contact us.";
            foreach (var credential in credentials)
            {
                credential.Transferred = true;
                credential.HackerName = newUser;
                await credentialRepository.Update(credential);
            }

            var transferCredentialHistory = new TransferCredentialHistoryEntity { Key = hackerEmail, ToEmail = newEmail, TransferredDateTime = DateTime.UtcNow };
            await _databaseLayer.Repo<TransferCredentialHistoryEntity>().Add(transferCredentialHistory);
            return string.Empty;
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
            var credentialRepository = _databaseLayer.Repo<CredentialEntity>();
            if (!int.TryParse(_configuration["NotificationPercent"], out var notificationPercent))
                notificationPercent = 15;
            if (!int.TryParse(_configuration["NotificationNumber"], out var notificationNumber))
                notificationNumber = 10;
            var totalCount = credentialRepository.DbSet.Count(x => x.AssetName == asset.Key);
            var freeCount = credentialRepository.DbSet.Count(x => x.AssetName == asset.Key && string.IsNullOrEmpty(x.HackerName));
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