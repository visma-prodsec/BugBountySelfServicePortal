using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Models.ViewModel;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface ICredentialService
    {
        Task<IEnumerable<UserCredentialViewModel>> GetCredentials(string hackerName, string hackerEmail, bool transferred);
        Task<IEnumerable<UserCredentialViewModel>> GetCredentialsByAdmin(string hackerName);
        Task<string> RequestCredentials(IEnumerable<string> assets, string hackerName, string hackerEmail);
        bool SendHackerEmail();
        Task<string> TransferCredentials(string hackerName, string hackerEmail, string newEmail);
    }
}