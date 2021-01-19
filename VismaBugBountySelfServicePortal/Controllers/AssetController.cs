using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VismaBugBountySelfServicePortal.Base;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Models.ViewModel;
using VismaBugBountySelfServicePortal.Services;

namespace VismaBugBountySelfServicePortal.Controllers
{
    [Authorize(Roles = Const.AdminRole)]
    public class AssetController : BaseController
    {
        private readonly ILogger<AssetController> _logger;
        private readonly IAssetService _assetService;
        private readonly ICredentialService _credentialService;
        private readonly IConfiguration _configuration;

        public AssetController(ILogger<AssetController> logger, IAssetService assetService, ICredentialService credentialService, IConfiguration configuration)
        {
            _logger = logger;
            _assetService = assetService;
            _credentialService = credentialService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Assets()
        {
            var assetModel = (await _assetService.GetAssets()).ToList().OrderBy(x => x.PercentAvailable);
            return View(assetModel);
        }

        public async Task<IActionResult> AddEditAsset(string assetName)
        {
            var model = new AddEditAssetViewModel { IsNew = true };
            if (string.IsNullOrWhiteSpace(assetName))
                return View(model);

            model = await _assetService.GetAssetDetails(assetName);
            if (model == null)
                ModelState.AddModelError("name", "Asset not found.");
            else model.IsNew = false;
            return View(model);
        }

        public async Task<IActionResult> AssetDetails(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return RedirectToAction("Assets");
            SetShowTransferredInfo();
            var model = await _assetService.GetAssetCredentials(assetName);
            if (model != null) return View(model);
            TempData["ErrorText"] = $"Asset {assetName} not found.";
            return RedirectToAction("Assets");
        }

        public async Task<IActionResult> ExportCsv(string assetName)
        {
            var asset = await _assetService.GetAssetDetails(assetName);
            if (asset == null)
            {
                TempData["ErrorText"] = $"Asset {assetName} not found.";
                return RedirectToAction("Assets");
            }

            var data = await _assetService.GetAssetCredentialsForExport(assetName);

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms, new UTF8Encoding(true));
            await using (var cw = new CsvWriter(sw, new CultureInfo("en-GB")))
            {
                await cw.WriteRecordsAsync(data);
            }
            return File(ms.ToArray(), "text/csv", $"assetCredentials_{asset.Description}_{DateTime.UtcNow.Ticks}.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAssetVisibility(string assetName, string isVisible)
        {
            var (message, isError) = await _assetService.UpdateAssetVisibility(assetName, isVisible == "on");
            if (!isError.HasValue) return RedirectToAction("Assets");
            if (isError.Value)
                TempData["ErrorText"] = message;
            else
                TempData["WarningText"] = message;

            return RedirectToAction("Assets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditAsset(AddEditAssetViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var errorText = await _assetService.AddOrUpdate(model);
            if (string.IsNullOrWhiteSpace(errorText))
                return RedirectToAction("Assets");

            ModelState.AddModelError("", errorText);
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelEdit()
        {
            return RedirectToAction("Assets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCredentials(string selectedAssetName, IFormFile postedFile)
        {
            _logger.LogInformation("Importing file");
            var (message, isError) = await _assetService.ImportFile(selectedAssetName, postedFile);
            _logger.LogInformation($"Importing file done. {(!string.IsNullOrWhiteSpace(message) ? $"Result: {message}" : "")}");
            if (isError)
                TempData["ErrorText"] = message;
            else
                TempData["SuccessText"] = $"Credentials for {selectedAssetName} imported successfully. {message}";
            return RedirectToAction("Assets");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SyncAssets()
        {
            await _assetService.SyncAssets();
            return RedirectToAction("Assets", "Asset");
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("api/sync")]
        public async Task<IActionResult> Sync([FromQuery] string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != _configuration["ApiKey"])
                return Unauthorized();
            await _assetService.SyncAssets();
            return Ok();
        }

        public async Task<IActionResult> UserAssetCredentials(string searchText = "")
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return View();
            SetShowTransferredInfo();
            var model = (await _credentialService.GetCredentialsByAdmin(searchText)).ToList();
            return model.FirstOrDefault(x => x.Credentials.Count > 0) == null ? View() : View(model.ToArray());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsset(string deleteAssetId)
        {
            await _assetService.DeleteAsset(deleteAssetId);
            return RedirectToAction("Assets");
        }

        public async Task<IActionResult> Statistics()
        {
            var model = await _assetService.GetStatistics();
            return View(model);
        }

        private void SetShowTransferredInfo()
        {
            ViewBag.ShowTransferredInfo = false;
            if (bool.TryParse(_configuration["ShowTransferredInfo"], out var showTransferredInfo))
                ViewBag.ShowTransferredInfo = showTransferredInfo;
        }
    }
}
