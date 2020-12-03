using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace VismaBugBountySelfServicePortal.Helpers
{
    public static class Helper
    {
        public static bool IsNonEmpty(this ITempDataDictionary dict, string key)
        {
            return dict.ContainsKey(key) && dict[key] != null && !string.IsNullOrWhiteSpace(dict[key].ToString());
        }
    }
}
