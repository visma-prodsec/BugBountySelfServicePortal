using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VismaBugBountySelfServicePortal.Models.ViewModel;

namespace VismaBugBountySelfServicePortal.Services
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetViewModel>> GetAssets();
        Task<UserCredentialViewModel> GetAssetCredentials(string selectedAsset);
        Task<AddEditAssetViewModel> GetAssetDetails(string selectedAsset);
        Task<(string message, bool? isError)> UpdateAssetVisibility(string assetName, bool isVisible);
        Task<string> AddOrUpdate(AddEditAssetViewModel model);
        Task<(string message, bool isError)> ImportFile(string assetName, IFormFile postedFile);
        Task SyncAssets();
        Task<IEnumerable<object>> GetAssetCredentialsForExport(string assetName);
        Task DeleteAsset(string deleteAssetId);
    }
}