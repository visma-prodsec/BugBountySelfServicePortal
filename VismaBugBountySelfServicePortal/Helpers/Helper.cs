using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace VismaBugBountySelfServicePortal.Helpers
{
    public static class Helper
    {
        public static bool IsNonEmpty(this ITempDataDictionary dict, string key)
        {
            return dict.ContainsKey(key) && dict[key] != null && !string.IsNullOrWhiteSpace(dict[key].ToString());
        }

        public static string GetEmail(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
        }
    }
}
