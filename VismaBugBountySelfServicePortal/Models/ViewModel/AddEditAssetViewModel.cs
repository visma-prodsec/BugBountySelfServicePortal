using System.ComponentModel.DataAnnotations;

namespace VismaBugBountySelfServicePortal.Models.ViewModel
{
    public class AddEditAssetViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Columns { get; set; }
        public bool IsNew { get; set; }
    }
}