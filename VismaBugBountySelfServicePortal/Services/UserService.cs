using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<UserEntity> _repository;
        private readonly IRepository<UserSessionEntity> _repositorySession;
        private readonly IRepository<UserSessionHistoryEntity> _repositorySessionHistory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionCookieName = "sessId";
        private static readonly Dictionary<string, string> TempSession = new Dictionary<string, string>();
        
        public UserService(IDatabaseLayer databaseLayer, IHttpContextAccessor httpContextAccessor)
        {
            _repository = databaseLayer.Repo<UserEntity>();
            _repositorySession = databaseLayer.Repo<UserSessionEntity>();
            _repositorySessionHistory = databaseLayer.Repo<UserSessionHistoryEntity>();
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> UserExist(string email)
        {
            return await _repository.Exists(email);
        }

        public async Task SaveSession(string email)
        {
            var sessionId = Guid.NewGuid();
            var userSession = new UserSessionEntity { Key = email, LoginDateTime = DateTime.UtcNow, SessionId = sessionId };
            await _repositorySession.Add(userSession);
            var userSessionHistory = new UserSessionHistoryEntity {Key = email, LoginDateTime = userSession.LoginDateTime, SessionId = sessionId};
            await _repositorySessionHistory.Add(userSessionHistory);
            var options = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                Expires = DateTime.Now.AddHours(1),
                IsEssential = true
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(SessionCookieName, sessionId.ToString(), options);
            TempSession[email] = sessionId.ToString();
        }

        public async Task RemoveSession(string email)
        {
            var sessions = await _repositorySession.FindAll(x => x.Key == email);
            foreach (var session in sessions)
            {
                await _repositorySession.Delete(session.Key);
            }
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(SessionCookieName);
        }

        public async Task<bool> IsValidSession(string email)
        {
            var cookie = _httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(SessionCookieName) ? _httpContextAccessor.HttpContext.Request.Cookies[SessionCookieName] : null;
            //cookie is not set right away, so we need this logic to check that session id is ok just before login
            if (TempSession.ContainsKey(email))
            {
                if (!string.IsNullOrWhiteSpace(cookie))
                    TempSession.Remove(email);
                else
                    cookie = TempSession[email];
            }

            if (string.IsNullOrWhiteSpace(cookie) || !Guid.TryParse(cookie, out var cookieGuid))
                return false;

            var session = await _repositorySession.FindOne(x => x.Key == email && x.SessionId == cookieGuid);
            return session != null;
        }
    }
}