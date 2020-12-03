using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VismaBugBountySelfServicePortal.Base
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BaseApiController : Controller
    {
    }
}
