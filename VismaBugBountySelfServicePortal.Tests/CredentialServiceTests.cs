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
    public class CredentialServiceTests : ServiceTestBase
    {
        private const string AssetName = "Asset Name";
        private ICredentialService _credentialService;

        [Fact]
        public async Task GetCredentials_CheckThatRightAssetsAreReturned()
        {
            var assets = new List<AssetEntity>
            {
                new() {Key = "NotVisible", IsVisible = false, Columns = "", Programs = "Program 1"},
                new() {Key = "NotOnH1", IsVisible = true, IsOnHackerOne = false, Columns = "", Programs = "Program 1"},
                new() {Key = "Public", IsVisible = true, IsOnHackerOne = true, IsOnPublicProgram = true, Columns = "", Programs = "Program X"},
                new() {Key = "NoPrograms", IsVisible = true, IsOnHackerOne = true, Columns = "", Programs = null},
                new() {Key = "OnProgram1", IsVisible = true, IsOnHackerOne = true, Programs = "Program 1", Columns = ""},
                new() {Key = "OnProgram2", IsVisible = true, IsOnHackerOne = true, Programs = "Program 2", Columns = ""},
                new() {Key = "OnProgram13", IsVisible = true, IsOnHackerOne = true, Programs = "Program 13", Columns = ""},
                new() {Key = "OnProgram1&13", IsVisible = true, IsOnHackerOne = true, Programs = $"Program 13{Const.DatabaseSeparator}Program 1", Columns = ""},
            };
            SetupData(assets, new List<string> { "Program 1", "Program 2" });

            var result = (await _credentialService.GetCredentials("hacker", "hackerEmail@intigriti.com", true)).ToList();

            result.ShouldBeEquivalentTo(new List<UserCredentialViewModel>
            {
                new(){AssetName = "Public", Columns = new(){""}, Credentials = new()},
                new(){AssetName = "OnProgram1", Columns = new(){""}, Credentials = new()},
                new(){AssetName = "OnProgram2", Columns = new(){""}, Credentials = new()},
                new(){AssetName = "OnProgram1&13", Columns = new(){""}, Credentials = new()},
            });
        }

        private void SetupData(List<AssetEntity> assets, List<string> hackerPrograms = null)
        {
            hackerPrograms ??= new List<string>();
            SetupData();
            SetupRepo(assets);
            SetupRepo(new List<CredentialEntity> { new() { AssetName = AssetName, HackerName = "H", Key = "1" }, new() { AssetName = AssetName, Key = "2" } });
            var providerServiceMock = new Mock<IProviderService>();
            var providerFactoryMock = new Mock<IProviderFactory>();
            providerFactoryMock.Setup(s => s.GetProviderService("intigriti.com")).Returns(providerServiceMock.Object);
            providerServiceMock.Setup(s => s.GetHackerProgramList(It.IsAny<string>()))
                .ReturnsAsync(hackerPrograms.Select(program => new ProgramModel { Name = program }).ToList());


            _credentialService = new CredentialService(DatabaseLayerMock.Object, null, null, Configuration, providerFactoryMock.Object, null);
        }
    }
}
