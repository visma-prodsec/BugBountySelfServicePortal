using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class UserSessionEntity : IEntity
    {
        [Column("Email")]
        public string Key { get; set; }
        public DateTime LoginDateTime { get; set; }
        public Guid SessionId { get; set; }
    }
}
