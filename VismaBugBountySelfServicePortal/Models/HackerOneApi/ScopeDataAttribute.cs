using System;
using Newtonsoft.Json;

namespace VismaBugBountySelfServicePortal.Models.HackerOneApi
{
    public class ScopeDataAttribute
    {
        [JsonProperty("asset_identifier")]
        public string AssetIdentifier { get; set; }
        public string Reference { get; set; }
        [JsonProperty("archived_at")]
        public DateTime? ArchivedAt { get; set; }
    }
}