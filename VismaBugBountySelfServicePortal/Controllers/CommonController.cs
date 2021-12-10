using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VismaBugBountySelfServicePortal.Base;
using VismaBugBountySelfServicePortal.Database;

namespace VismaBugBountySelfServicePortal.Controllers
{
    [Route("api")]
    [ApiController]
    public class CommonController : BaseApiController
    {
        private readonly IDataSeeder _dataSeeder;
        private readonly ILogger<CommonController> _logger;

        public CommonController(IDataSeeder seeder, ILogger<CommonController> logger, IConfiguration configuration): base(configuration)
        {
            _dataSeeder = seeder;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint to check if the service is alive. This endpoint will allow anonymous access.It will return Environment.MachineName where there service is run and the Service Version.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("ping")]
        public ActionResult<string> Get()
        {
            return $"pong ({Assembly.GetExecutingAssembly().GetName().Version})";
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("migrate")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> Migrate()
        {
            if (!ValidateApiKey())
                return Unauthorized();
            _logger.LogInformation("Migrating...");
            await _dataSeeder.MigrateDatabase();
            _logger.LogInformation("Migration done.");

            return Ok();
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("seed")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> Seed()
        {
            if (!ValidateApiKey())
                return Unauthorized();
            _logger.LogInformation("Seeding...");
            await _dataSeeder.LoadSeed();
            _logger.LogInformation("Seed done.");
            return Ok();
        }
    }
}
