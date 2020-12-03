using System.Threading.Tasks;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface IUserService
    {
        Task<bool> UserExist(string email);
        Task SaveSession(string email);
        Task RemoveSession(string email);
        Task<bool> IsValidSession(string email);
    }
}