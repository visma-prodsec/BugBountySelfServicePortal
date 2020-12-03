using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VismaBugBountySelfServicePortal.Base;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Models.ViewModel;
using VismaBugBountySelfServicePortal.Services;

namespace VismaBugBountySelfServicePortal.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (User.HasClaim(ClaimTypes.Role, Const.AdminRole))
                return RedirectToAction("Assets", "Asset");
            if (User.HasClaim(ClaimTypes.Role, Const.HackerRole))
                return RedirectToAction("UserCredentials", "Credential");
            return View("Unauthorized");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error([Bind(Prefix = "id")] int statusCode = 0)
        {
            // Retrieve error information in case of internal errors
            var path = HttpContext
                .Features
                .Get<IExceptionHandlerPathFeature>();
            if (path == null)
            {
                _logger.LogError(new Exception(), "An exception occured, status code:" + statusCode);
                return View(new ErrorViewModel { HttpStatusCode = statusCode });
            }

            // Use the information about the exception 
            var exception = path.Error;
            var pathString = path.Path;
            _logger.LogError(exception, "An exception occurred on: " + pathString);
            return View(new ErrorViewModel { ErrorMessage = "An error occurred on processing your request. Please contact us." });
        }

        public IActionResult Unauthorized(string returnUrl)
        {
            _logger.LogWarning($"Unauthorized user {User?.Identity?.Name} {User?.Claims?.FirstOrDefault(x => x.Type == "email")?.Value} tries to access {returnUrl}");
            return View("Unauthorized");
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task Logout()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                await HttpContext.SignOutAsync("Cookies");
                await HttpContext.SignOutAsync("OpenIdConnect");
                var email = User.Claims.GetEmail();
                await _userService.RemoveSession(email);
            }
        }
    }
}
