using System.Threading.Tasks;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Base.Database.Repository
{
    public interface IDataRepository
    {
        Task<IEntity> GetOneEntity(string id);
        bool SkipSaving { get; set; }
    }
}