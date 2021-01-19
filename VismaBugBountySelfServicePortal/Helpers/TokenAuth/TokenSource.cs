using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace VismaBugBountySelfServicePortal.Helpers.TokenAuth
{
    public class TokenSource : ITokenSource
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string[] _serviceScopes;
        private readonly string _url;
        private readonly bool _useDiscovery;
        private string _tokenEndpoint;

        
        public ILogger Logger { get; set; }

        public TokenSource(string clientId, string clientSecret, string[] serviceScopes, string url, bool useDiscovery = true)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _serviceScopes = serviceScopes;
            _url = url;
            _useDiscovery = useDiscovery;
        }

        private string TokenEndpoint
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_tokenEndpoint))
                    return _tokenEndpoint;
                if (!_useDiscovery)
                    _tokenEndpoint = _url;
                else
                {
                    using var client = new HttpClient();
                    var task = Task.Run(async () => await client.GetDiscoveryDocumentAsync(_url));
                    _tokenEndpoint = task.Result.TokenEndpoint;
                }

                return _tokenEndpoint;
            }
        }

        public Token GetToken(bool refresh = false)
        {
            try
            {
                using var httpClient = new HttpClient();
                var task = Task.Run(async () => await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = TokenEndpoint,
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Scope = string.Join(" ", _serviceScopes)
                }));

                var token = task.Result;
                if (!token.IsError)
                {
                    return new Token
                    {
                        AccessToken = token.AccessToken,
                        TokenType = token.TokenType,
                        ValidUntil = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
                    };
                }

                Logger.LogError($"Token retrieval failed: {token.ErrorDescription}");
                return null;
            }
            catch (Exception e)
            {
                 Logger.LogError("TokenSource.GetToken failed with an exception", e);
                return null;
            }

        }
    }
}
