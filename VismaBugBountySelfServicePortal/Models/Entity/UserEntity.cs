using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class UserEntity : IEntity
    {
        [Key, Column("Email")]
        public string Key { get; set; }
    }
}
