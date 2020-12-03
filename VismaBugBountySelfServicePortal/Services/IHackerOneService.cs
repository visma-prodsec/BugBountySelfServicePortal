using System.Collections.Generic;
using System.Threading.Tasks;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface IHackerOneService
    {
        Task<bool> IsHackerInPrivateProgram(string hackerName);
        Task<HashSet<string>> GetAssets(bool privateAssets);
    }
}
