using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class CredentialValueEntity : IEntity
    {
        [Column("SetId")]
        public string Key { get; set; }
        public int RowNumber { get; set; }
        public string AssetName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
    }
}
