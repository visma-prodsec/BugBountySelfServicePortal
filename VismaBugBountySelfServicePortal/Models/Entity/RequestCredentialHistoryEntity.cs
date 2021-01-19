using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class RequestCredentialHistoryEntity : IEntity
    {
        [Column("SetId")]
        public string Key { get; set; }
        public string AssetName { get; set; }
        public string HackerName { get; set; }
        public DateTime RequestDateTime { get; set; }
    }
}
