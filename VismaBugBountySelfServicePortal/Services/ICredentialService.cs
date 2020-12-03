using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Models.ViewModel;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface ICredentialService
    {
        Task<IEnumerable<UserCredentialViewModel>> GetCredentials(string hackerName);
        Task<IEnumerable<UserCredentialViewModel>> GetCredentialsByAdmin(string hackerName);
        Task<string> RequestCredentials(IEnumerable<string> assets, string hackerName, string hackerEmail);
        bool SendHackerEmail();
    }
}