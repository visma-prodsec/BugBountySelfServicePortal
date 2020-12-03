using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VismaBugBountySelfServicePortal.Base;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Services;

namespace VismaBugBountySelfServicePortal.Controllers
{
    [Authorize(Roles = Const.HackerRole)]
    public class CredentialController : BaseController
    {
        private readonly ICredentialService _credentialService;
        private string HackerName => User.Claims.FirstOrDefault(x => x.Type == Const.ClaimTypeHackerName)?.Value ?? "";

        public CredentialController(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }

        public async Task<IActionResult> UserCredentials()
        {
            var model = await _credentialService.GetCredentials(HackerName);
            return View(model.ToArray());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCredentials(string selectedAsset)
        {
            var result = await _credentialService.RequestCredentials(new List<string> { selectedAsset },
                    HackerName, User.Claims.GetEmail());
            if (!string.IsNullOrWhiteSpace(result))
                TempData["ErrorText"] = result;
            else TempData["SuccessText"] = _credentialService.SendHackerEmail() ? $"Credentials for {selectedAsset} sent by email. You can find them also in this page." : "Credentials added.";
            return RedirectToAction("UserCredentials");
        }
    }
}
