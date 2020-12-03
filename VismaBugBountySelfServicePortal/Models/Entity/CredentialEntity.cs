using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class CredentialEntity : IEntity
    {
        [Column("SetId")]
        public string Key { get; set; }
        public string AssetName { get; set; }
        public string HackerName { get; set; }
        [NotMapped]
        public List<CredentialValueEntity> Rows { get; set; } = new List<CredentialValueEntity>();
    }
}
