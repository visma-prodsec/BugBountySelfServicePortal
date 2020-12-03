using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class UserSessionEntity : IEntity
    {
        [Key, Column("Email")]
        public string Key { get; set; }
        public DateTime LoginDateTime { get; set; }
    }
}
