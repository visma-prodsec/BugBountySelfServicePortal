using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace VismaBugBountySelfServicePortal.Base
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BaseApiController : Controller
    {
        private const string ApiKeyHeader = "x-api-key";
        private readonly IConfiguration _configuration;

        public BaseApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected bool ValidateApiKey(string apiKey = null)
        {
            apiKey = GetApiKey(apiKey);
            return !string.IsNullOrWhiteSpace(apiKey) && apiKey == _configuration["ApiKey"];
        }

        private string GetApiKey(string apiKey, bool useHeader = true)
        {
            if (!useHeader || !Request.Headers.ContainsKey(ApiKeyHeader))
                return apiKey;
            if (Request.Headers.ContainsKey(ApiKeyHeader))
                return Request.Headers[ApiKeyHeader];
            return apiKey;
        }
    }
}
