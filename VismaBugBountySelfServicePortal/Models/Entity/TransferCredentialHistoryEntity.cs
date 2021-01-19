using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class TransferCredentialHistoryEntity : IEntity
    {
        [Column("FromEmail")]
        public string Key { get; set; }
        public string ToEmail { get; set; }
        public DateTime TransferredDateTime { get; set; }
    }
}
