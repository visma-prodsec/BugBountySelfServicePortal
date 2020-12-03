using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VismaBugBountySelfServicePortal.Models.Entity
{
    public class AssetEntity : IEntity
    {
        [Key, Column("AssetName")]
        public string Key { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }
        public bool IsOnHackerOne { get; set; }
        public bool IsOnPublicProgram { get; set; }
        public string Columns { get; set; }
    }
}
