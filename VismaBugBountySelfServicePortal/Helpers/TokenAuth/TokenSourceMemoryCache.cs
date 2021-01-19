using System;
using Microsoft.Extensions.Logging;

namespace VismaBugBountySelfServicePortal.Helpers.TokenAuth
{
    /// <summary>
    /// CachedTokenSource caches the Token object and retrieves a new one only when previous token is invalid, or doesn't exist
    /// </summary>
    public class TokenSourceMemoryCache : ITokenSource
    {
        // force token renewal to happen before the expiration time
        private const int ForceTokenRenewalBeforeExpirationInSeconds = 10;

        // Get and Set Logger. If nothing is set use internal NullLogger, that writes no log
        public ILogger Logger { get; set; }

        private readonly object _tokenLock = new object();
        private readonly ITokenSource _actualTokenSource;
        private Token _token;

        public TokenSourceMemoryCache(ITokenSource actualTokenSource)
        {
            _actualTokenSource = actualTokenSource;
        }
        public Token GetToken(bool refresh = false)
        {
            bool IsTokenValid()
            {
                // subtract 10 seconds from valid until date time to make sure token is renewed before expiry
                return _token != null && _token.ValidUntil.AddSeconds(-ForceTokenRenewalBeforeExpirationInSeconds) > DateTime.UtcNow;
            }

            if (!IsTokenValid() || refresh)
                lock (_tokenLock)
                {
                    // to verify we don't refresh right after previous lock's been released
                    if (!IsTokenValid() || refresh)
                    {
                        Logger.LogInformation("Token in cache is expired or not set, retrieve a new one");
                        _token = _actualTokenSource.GetToken();
                    }
                }
            return _token;
        }
    }
}