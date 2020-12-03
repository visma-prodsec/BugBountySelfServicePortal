using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<UserEntity> _repository;

        public UserService(IDatabaseLayer databaseLayer)
        {
            _repository = databaseLayer.Repo<UserEntity>();
        }
        public async Task<bool> UserExist(string email)
        {
            return await _repository.Exists(email);
        }
    }
}