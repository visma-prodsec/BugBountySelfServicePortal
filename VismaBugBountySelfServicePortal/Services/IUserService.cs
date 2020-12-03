using System.Threading.Tasks;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface IUserService
    {
        Task<bool> UserExist(string email);
    }
}