﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VismaBugBountySelfServicePortal.Helpers.TokenAuth;

namespace VismaBugBountySelfServicePortal.Services
{
    public class IntigritiService : IProviderService
    {

        private const int ConfidentialityLevelInviteOnlyId = 1;
        private const int ConfidentialityLevelApplicationId = 2;
        private const int ConfidentialityLevelRegisteredId = 3;
        private const int ConfidentialityLevelPublicId = 4;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IntigritiService> _logger;
        private string BaseUrl => _configuration["IntigritiAPIUrl"];

        private HttpClient _client;

        private static ITokenSource _tokenSource;
        private static readonly object TokenLocker = new();
        private static readonly object ClientLocker = new();
        #region getClient
        private ITokenSource TokenSource
        {
            get
            {
                if (_tokenSource == null)
                    lock (TokenLocker)
                    {
                        if (_tokenSource == null)
                        {
                            var tokenSource = new TokenSource(
                                _configuration["IntigritiClientId"],
                                _configuration["IntigritiSecret"],
                                new[] { _configuration["IntigritiClientScope"] },
                                _configuration["IntigritiTokenUrl"])
                            {
                                Logger = _logger
                            };
                            _tokenSource = new TokenSourceMemoryCache(tokenSource) { Logger = _logger };
                        }
                    }

                return _tokenSource;
            }
        }

        #endregion getClient

        public IntigritiService(IConfiguration configuration, ILogger<IntigritiService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<bool> IsHackerInPrivateProgram(string hackerName)
        {
            var programs = await GetPrograms();
            foreach (var (programId, _) in programs.Where(p => p.Value == ConfidentialityLevelInviteOnlyId || p.Value == ConfidentialityLevelApplicationId))
            {
                var urlProgram = $"{BaseUrl}/v1.2/programs/{programId}/researchers/{hackerName}/access";

                var (content, status) = await GetStringData(urlProgram);
                if (status != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error on calling API to find info for: {hackerName}, status: {status}, content: {content}");
                    return false;
                }

                var records = JObject.Parse(content);
                if (records["canCreateSubmission"].Value<bool>())
                    return true;
            }

            return false;
        }

        public async Task<HashSet<string>> GetAssets(bool privateAssets)
        {
            var data = new HashSet<string>();
            var programs = await GetPrograms();
            var ids = privateAssets ? new HashSet<int> { ConfidentialityLevelInviteOnlyId, ConfidentialityLevelApplicationId } : new HashSet<int> { ConfidentialityLevelPublicId, ConfidentialityLevelRegisteredId };
            foreach (var (programId, _) in programs.Where(p => ids.Contains(p.Value)))
            {
                var urlProgram = $"{BaseUrl}/v1.2/programs/{programId}";

                var (content, status) = await GetStringData(urlProgram);
                if (status != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error on calling API to find info assets on program: {programId} {(privateAssets ? "private" : "public")}, status: {status}, content: {content}");
                    return new HashSet<string>();
                }
                var records = JObject.Parse(content);
                foreach (var domain in records["domains"])
                foreach (var record in domain["content"])
                    data.Add(record["endpoint"].Value<string>());
            }
            
            return data;
        }

        private async Task<Dictionary<string, int>> GetPrograms()
        {
            var url = $"{BaseUrl}/v1.2/programs";
            var (content, status) = await GetStringData(url);
            if (status != HttpStatusCode.OK)
            {
                _logger.LogError($"Error on calling API to find programs, status: {status}, content: {content}");
                return new Dictionary<string, int>();
            }

            var records = JArray.Parse(content);

            return records.Where(record => record["status"]["value"].Value<string>() == "Open")
                .ToDictionary(record => record["id"].Value<string>(), record => record["confidentialityLevel"]["id"].Value<int>());
        }
        private async Task<(string Content, HttpStatusCode Status)> GetStringData(string url)
        {
            try
            {
                var client = GetHttpClient();
                var response = await client.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.Unauthorized)
                    return (await response.Content.ReadAsStringAsync(), response.StatusCode);
                TokenSource.GetToken(true);
                client = GetHttpClient();
                response = await client.GetAsync(url);
                return (await response.Content.ReadAsStringAsync(), response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error on getting data from url: {url}");
                throw;
            }
        }

        private HttpClient GetHttpClient()
        {
            var header = GetAuthenticationHeaderValue();
            lock (ClientLocker)
            {
                if (header != null && (Client.DefaultRequestHeaders.Authorization == null || Client.DefaultRequestHeaders.Authorization.Parameter != header.Parameter))
                    Client.DefaultRequestHeaders.Authorization = header;
                return Client;
            }
        }

        private HttpClient Client
        {
            get
            {
                if (_client == null)
                    lock (ClientLocker)
                        if (_client == null)
                            _client = new HttpClient();
                return _client;
            }
        }

        private AuthenticationHeaderValue GetAuthenticationHeaderValue()
        {
            var token = TokenSource.GetToken();
            return token != null ? new AuthenticationHeaderValue(token.TokenType, token.AccessToken) : null;
        }
    }
}