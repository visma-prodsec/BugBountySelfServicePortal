using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VismaBugBountySelfServicePortal.Base
{
    [Authorize]
    public class BaseController : Controller
    {
    }
}