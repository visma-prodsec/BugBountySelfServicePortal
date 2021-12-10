using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VismaBugBountySelfServicePortal.Base;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Services;

namespace VismaBugBountySelfServicePortal.Controllers
{
    [Authorize]
    public class CredentialController : BaseController
    {
        private readonly ICredentialService _credentialService;
        private readonly IConfiguration _configuration;
        private string HackerName
        {
            get
            {
                var hackerName = HttpContext.Session.GetString(Const.ClaimTypeHackerName);
                if (!string.IsNullOrWhiteSpace(hackerName) && User.IsInRole(Const.AdminRole))
                {
                    ViewBag.HackerName = hackerName;
                    return hackerName;
                }

                return User.Claims.FirstOrDefault(x => x.Type == Const.ClaimTypeHackerName)?.Value ?? "";
            }
        }

        private string HackerEmail
        {
            get
            {
                var hackerName = HttpContext.Session.GetString(Const.ClaimTypeHackerName);
                if (!string.IsNullOrWhiteSpace(hackerName) && User.IsInRole(Const.AdminRole))
                    return $"{hackerName}@{_configuration["ProviderEmailDomain"]}";
                return User.Claims.GetEmail();
            }
        }

        private bool IsObsoleteUserDomain => User.HasClaim(c => c.Type == Const.ClaimTypeObsoleteDomain);

        public CredentialController(ICredentialService credentialService, IConfiguration configuration)
        {
            _credentialService = credentialService;
            _configuration = configuration;
        }

        public async Task<IActionResult> UserCredentials()
        {
            if (IsObsoleteUserDomain)
            {
                var data = (await _credentialService.GetCredentials(HackerName, HackerEmail, false)).ToList();
                var anyCredentialsToTransfer = data.Any(x => x.Credentials.Any());
                TempData["MovedMessage"] = anyCredentialsToTransfer ? _configuration["MovedMessage"] : _configuration["MovedMessageNoCredentials"];
                ViewBag.EmailLabel = _configuration["TransferEmailLabel"];
                ViewBag.AnyCredentialsToTransfer = anyCredentialsToTransfer;
                return View(data.ToArray());
            }
            var model = await _credentialService.GetCredentials(HackerName, HackerEmail, true);
            return View(model.ToArray());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCredentials(string selectedAsset)
        {
            if (IsObsoleteUserDomain)
            {
                TempData["ErrorText"] = "Invalid action.";
                return RedirectToAction("UserCredentials");
            }
            var result = await _credentialService.RequestCredentials(new List<string> { selectedAsset }, HackerName, HackerEmail);
            if (!string.IsNullOrWhiteSpace(result))
                TempData["ErrorText"] = result;
            else TempData["SuccessText"] = _credentialService.SendHackerEmail() ? $"Credentials for {selectedAsset} sent by email. You can find them also in this page." : "Credentials added.";
            return RedirectToAction("UserCredentials");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferCredentials(string newEmail)
        {
            if (!IsObsoleteUserDomain)
            {
                TempData["ErrorText"] = "Invalid action.";
                return RedirectToAction("UserCredentials");
            }

            var result = await _credentialService.TransferCredentials(HackerName, HackerEmail, newEmail);
            if (!string.IsNullOrWhiteSpace(result))
                TempData["ErrorText"] = result;
            else TempData["SuccessText"] = _configuration["CredentialTransferredMessage"];
            return RedirectToAction("UserCredentials");
        }
    }
}
