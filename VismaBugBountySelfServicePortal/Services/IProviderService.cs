using System.Collections.Generic;
using System.Threading.Tasks;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface IProviderService
    {
        Task<bool> IsHackerInPrivateProgram(string hackerName);
        Task<HashSet<string>> GetAssets(bool privateAssets);
    }
}
