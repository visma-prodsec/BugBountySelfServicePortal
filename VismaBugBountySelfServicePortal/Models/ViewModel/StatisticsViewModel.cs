using System.Collections.Generic;

namespace VismaBugBountySelfServicePortal.Models.ViewModel
{
    public class StatisticsViewModel
    {
        public int ResearchersTransferred { get; set; }
        public int ResearchersTotal { get; set; }
        public Dictionary<int, int> CredentialsRequestedCount { get; set; }
        public Dictionary<int, int> LoginCounts { get; set; }
        public Dictionary<string, int> CredentialsPerResearcherCount { get; set; }
        public Dictionary<string, int> CredentialsPerAssetCount { get; set; }
    }
}