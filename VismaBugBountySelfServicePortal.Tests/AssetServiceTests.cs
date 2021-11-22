using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Models.Entity;
using VismaBugBountySelfServicePortal.Models.ViewModel;
using VismaBugBountySelfServicePortal.Services;
using Xunit;

namespace VismaBugBountySelfServicePortal.Tests
{
    public class AssetServiceTests : ServiceTestBase
    {
        private const string AssetName = "Asset Name";
        private IAssetService _assetService;
        [Fact]
        public async Task GetAssets()
        {
            var assets = new List<AssetEntity>
            {
                new()
                {
                    Key = AssetName,
                    Description = "desc",
                    IsVisible = true,
                    IsOnHackerOne = true,
                    IsOnPublicProgram = true,
                    Programs = $"Program 1{Const.DatabaseSeparator}Program 2"
                }
            };
            SetupData(assets);

            var result = (await _assetService.GetAssets()).ToList();

            result.ShouldBeEquivalentTo(new List<AssetViewModel>
            {
                new()
                {
                    Name = AssetName,
                    Description = "desc",
                    Free = 1,
                    IsOnHackerOne = true,
                    IsOnPublicProgram = true,
                    IsVisible = true,
                    Programs = $"Program 1{Const.UISeparator}Program 2",
                    Total = 2
                }
            });

        }
        
        [Fact]
        public async Task SyncAssets_NoUpdate()
        {
            var assets = new List<AssetEntity>
            {
                new()
                {
                    Key = AssetName,
                    Description = "desc",
                    IsVisible = true,
                    IsOnHackerOne = true,
                    IsOnPublicProgram = true,
                    Programs = "Public"
                }
            };
            SetupData(assets, publicAssets: new List<string> { AssetName });

            await _assetService.SyncAssets();

            UpdatedEntities.Count.ShouldBe(0);
        }

        [Fact]
        public async Task SyncAssets_UpdateProgram()
        {
            var assets = new List<AssetEntity>
            {
                new()
                {
                    Key = AssetName,
                    Description = "desc",
                    IsVisible = true,
                    IsOnHackerOne = true,
                    IsOnPublicProgram = true,
                    Programs = "No Public"
                }
            };
            SetupData(assets, privateAssets: new List<string> { AssetName }, publicAssets: new List<string> { AssetName });

            await _assetService.SyncAssets();

            UpdatedEntities.Count.ShouldBe(1);
            var asset = UpdatedEntities[0].Entity as AssetEntity;
            asset.ShouldNotBeNull();
            asset.Programs.ShouldBe($"Public{Const.DatabaseSeparator}Private");
        }

        [Fact]
        public async Task SyncAssets_UpdateIsOnHackerOne_ToFalse()
        {
            var assets = new List<AssetEntity>
            {
                new()
                {
                    Key = AssetName,
                    Description = "desc",
                    IsVisible = true,
                    IsOnHackerOne = true,
                    IsOnPublicProgram = true,
                    Programs = ""
                }
            };
            SetupData(assets);

            await _assetService.SyncAssets();

            UpdatedEntities.Count.ShouldBe(1);
            var asset = UpdatedEntities[0].Entity as AssetEntity;
            asset.ShouldNotBeNull();
            asset.IsOnHackerOne.ShouldBeFalse();
        }
        [Fact]
        public async Task SyncAssets_UpdateIsOnHackerOne_ToTrue()
        {
            var assets = new List<AssetEntity>
            {
                new()
                {
                    Key = AssetName,
                    Description = "desc",
                    IsVisible = true,
                    IsOnHackerOne = false,
                    IsOnPublicProgram = true,
                    Programs = "Public"
                }
            };
            SetupData(assets, publicAssets: new List<string> { AssetName });

            await _assetService.SyncAssets();

            UpdatedEntities.Count.ShouldBe(1);
            var asset = UpdatedEntities[0].Entity as AssetEntity;
            asset.ShouldNotBeNull();
            asset.IsOnHackerOne.ShouldBeTrue();
        }
        [Fact]
        public async Task SyncAssets_UpdateIsOnPublicProgram_ToFalse()
        {
            var assets = new List<AssetEntity>
            {
                new()
                {
                    Key = AssetName,
                    Description = "desc",
                    IsVisible = true,
                    IsOnHackerOne = true,
                    IsOnPublicProgram = true,
                    Programs = "Private"
                }
            };
            SetupData(assets, privateAssets: new List<string> { AssetName });

            await _assetService.SyncAssets();

            UpdatedEntities.Count.ShouldBe(1);
            var asset = UpdatedEntities[0].Entity as AssetEntity;
            asset.ShouldNotBeNull();
            asset.IsOnPublicProgram.ShouldBeFalse();
        }

        private void SetupData(List<AssetEntity> assets, List<string> privateAssets = null, List<string> publicAssets = null)
        {
            SetupData();
            privateAssets ??= new List<string>();
            publicAssets ??= new List<string>();

            static HashSet<(string Name, string Program)> ComputeHashSet(IEnumerable<string> items, string program)
            {
                var hashSet = new HashSet<(string Name, string Program)>();
                foreach (var asset in items)
                {
                    hashSet.Add((asset, program));
                }

                return hashSet;
            }
            SetupRepo(assets);
            SetupRepo(new List<CredentialEntity> { new() { AssetName = AssetName, HackerName = "H", Key = "1" }, new() { AssetName = AssetName, Key = "2" } });
            var providerServiceMock = new Mock<IProviderService>();
            var providerFactoryMock = new Mock<IProviderFactory>();
            providerFactoryMock.Setup(s => s.GetProviderService(It.IsAny<string>())).Returns(providerServiceMock.Object);
            providerServiceMock.Setup(s => s.GetAssets(false)).ReturnsAsync(ComputeHashSet(publicAssets, "Public"));
            providerServiceMock.Setup(s => s.GetAssets(true)).ReturnsAsync(ComputeHashSet(privateAssets, "Private"));

            _assetService = new AssetService(DatabaseLayerMock.Object, Configuration, null, providerFactoryMock.Object);
        }
    }
}
