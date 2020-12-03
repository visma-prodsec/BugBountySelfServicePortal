using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VismaBugBountySelfServicePortal.Services;

namespace VismaBugBountySelfServicePortal.Helpers
{
    class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ILogger<ClaimsTransformer> _logger;
        

        public ClaimsTransformer(IConfiguration configuration, IUserService userService, ILogger<ClaimsTransformer> logger)
        {
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
        }
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var id = (ClaimsIdentity)principal?.Identity;
            if (id == null)
                return principal;
            if (id.HasClaim(x => x.Type == ClaimTypes.Role) || !id.HasClaim(x => x.Type == "email"))
                return principal;
            var email = id.Claims.GetEmail();

            if (string.IsNullOrWhiteSpace(email) || !email.EndsWith(_configuration["HackerEmailDomain"]) && !email.EndsWith(_configuration["AdminEmailDomain"]))
            {
                _logger.LogWarning($"Unauthorized user tried to logon: {email}");
                return principal;
            }
            
            var role = "";
            if (email.EndsWith(_configuration["AdminEmailDomain"]))
            {
                if (await _userService.UserExist(email))
                    role = Const.AdminRole;
                else
                {
                    _logger.LogWarning($"Unauthorized user tried to logon: {email}");
                    return principal;
                }
            }

            if (email.EndsWith(_configuration["HackerEmailDomain"]))
            {
                role = Const.HackerRole;
                id.AddClaim(new Claim(Const.ClaimTypeHackerName, email.Split("@")[0].Split("+")[0]));
            }

            ((ClaimsIdentity)principal.Identity)?.AddClaim(new Claim(ClaimTypes.Role, role));
            return principal;
        }
    }
}