using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Models.ViewModel;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface IProviderService
    {
        Task<List<ProgramModel>> GetHackerProgramList(string hackerName);
        Task<HashSet<(string Name, string Program)>> GetAssets(bool privateAssets);
    }
}
