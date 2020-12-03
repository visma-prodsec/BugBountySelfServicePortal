using System;

namespace VismaBugBountySelfServicePortal.Models.ViewModel
{
    public class AssetViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }
        public bool IsOnPublicProgram { get; set; }
        public bool IsOnHackerOne { get; set; }
        public int Free { get; set; }
        public int Total { get; set; }
        public decimal PercentAvailable => Total == 0 ? 0 : Math.Round(100m * Free / Total, 2);
    }
}