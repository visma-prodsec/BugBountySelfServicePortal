using System;
using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<UserEntity> _repository;
        private readonly IRepository<UserSessionEntity> _repositorySession;

        public UserService(IDatabaseLayer databaseLayer)
        {
            _repository = databaseLayer.Repo<UserEntity>();
            _repositorySession = databaseLayer.Repo<UserSessionEntity>();
        }
        public async Task<bool> UserExist(string email)
        {
            return await _repository.Exists(email);
        }

        public async Task SaveSession(string email)
        {
            var userSession = new UserSessionEntity { Key = email, LoginDateTime = DateTime.UtcNow };
            var session = await _repositorySession.GetOne(email);
            if (session == null)
            {
                await _repositorySession.Add(userSession);
                return;
            }
            session.LoginDateTime = DateTime.UtcNow;
            await _repositorySession.Update(session);
        }

        public async Task RemoveSession(string email)
        {
            await _repositorySession.Delete(email);
        }

        public async Task<bool> IsValidSession(string email)
        {
            var session = await _repositorySession.GetOne(email);
            return session != null;
        }
    }
}