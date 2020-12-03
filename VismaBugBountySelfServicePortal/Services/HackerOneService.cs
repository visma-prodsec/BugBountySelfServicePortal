using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VismaBugBountySelfServicePortal.Models.HackerOneApi;

namespace VismaBugBountySelfServicePortal.Services
{
    public class HackerOneService : IHackerOneService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HackerOneService> _logger;
        private string BaseUrl => _configuration["HackerOneUrl"];

        public HackerOneService(IConfiguration configuration, ILogger<HackerOneService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> IsHackerInPrivateProgram(string hackerName)
        {
            var url = string.Format($"{BaseUrl}/{_configuration["UsersUrl"]}", hackerName);
            var (content, status) = await GetStringData(url, true);
            if (status != HttpStatusCode.OK)
                return false;
            var textSearch = JObject.Parse(content);
            var results = textSearch?["data"]?["relationships"]?["participating_programs"]?["data"]?.Children().ToList() ?? new List<JToken>();
            return results.Count == 1;
        }

        public async Task<HashSet<string>> GetAssets(bool privateAssets)
        {
            var urlProgram = $"{BaseUrl}/{_configuration["ProgramsUrl"]}";

            var (content, status) = await GetStringData(urlProgram, privateAssets);
            if (status != HttpStatusCode.OK)
                return null;
            var textSearch = JObject.Parse(content);
            var programId = textSearch?["data"]?.First()?["id"]?.Value<string>() ?? "";
            if(string.IsNullOrWhiteSpace(programId))
                return new HashSet<string>();
            var assetsData = await GetAssetsList(programId, privateAssets);
            return assetsData.Where(x => x.Attributes.ArchivedAt == null).Select(x => x.Attributes.AssetIdentifier).ToHashSet();
        }

        private async Task<List<ScopeData>> GetAssetsList(string programId, bool privateAssets)
        {
            var assetsUrl = string.Format($"{BaseUrl}/{_configuration["ScopesUrl"]}", programId);
            var projects = new List<ScopeData>();
            do
            {
                var data = await GetAssetsPaginated(assetsUrl, privateAssets);
                projects.AddRange(data?.Data ?? new List<ScopeData>());
                assetsUrl = data?.Links.Next;

            } while (!string.IsNullOrWhiteSpace(assetsUrl));

            return projects;

        }

        private async Task<Scope> GetAssetsPaginated(string url, bool privateAssets)
        {
            var (content, status) = await GetStringData(url, privateAssets);
            return status != HttpStatusCode.OK ? null : JsonConvert.DeserializeObject<Scope>(content);
        }

        private async Task<(string Content, HttpStatusCode Status)> GetStringData(string url, bool isPrivate)
        {
            var username = isPrivate ? _configuration["PrivateProgramUsername"] : _configuration["PublicProgramUsername"];
            var password = isPrivate ? _configuration["PrivateProgramPassword"] : _configuration["PublicProgramPassword"];
            try
            {
                using var httpClientHandler = new HttpClientHandler();
                using var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Encode($"{username}:{password}"));
                var response = await httpClient.GetAsync(url);

                return (await response.Content.ReadAsStringAsync(), response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error on getting data from url: {url}");
                throw;
            }
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}