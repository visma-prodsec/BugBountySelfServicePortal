namespace VismaBugBountySelfServicePortal.Services
{
    public interface IProviderFactory
    {
        IProviderService GetProviderService(string userDomain);
    }
}