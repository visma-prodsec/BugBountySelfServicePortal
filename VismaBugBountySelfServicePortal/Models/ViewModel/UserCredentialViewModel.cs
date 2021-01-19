using System.Collections.Generic;

namespace VismaBugBountySelfServicePortal.Models.ViewModel
{
    public class UserCredentialViewModel
    {
        public string AssetName { get; set; }
        public string Description { get; set; }
        
        public List<string> Columns { get; set; }
        public List<CredentialsViewModel> Credentials { get; set; }
    }

    public class CredentialsViewModel
    {
        public string SetId { get; set; }
        public string HackerName { get; set; }
        public bool? Transferred { get; set; }
        public Dictionary<int, List<(string ColumnName, string ColumnValue)>> Rows;
    }
}