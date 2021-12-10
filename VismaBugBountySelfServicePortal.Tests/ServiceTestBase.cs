using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Models.Entity;
using VismaBugBountySelfServicePortal.Tests.TestImplementations;
using static VismaBugBountySelfServicePortal.Tests.Helper;

namespace VismaBugBountySelfServicePortal.Tests
{
    public class ServiceTestBase
    {
        protected readonly List<(ActionEntity Action, IEntity Entity)> UpdatedEntities = new();
        protected readonly Mock<IDatabaseLayer> DatabaseLayerMock = new();
        protected IConfiguration Configuration;

        protected virtual void SetupData()
        {
            UpdatedEntities.Clear();
            Configuration = new TestConfiguration
            {
                ["ProviderEmailDomain"] = "test",
                ["SecurityTeamUsers"] ="secUser"
            };
        }

        protected void SetupRepo<TS>(TS item = null) where TS : class, IEntity, new()
        {
            SetupRepo(item == null ? 0 : 1, item == null ? new List<TS>() : new List<TS> { item });
        }
        protected void SetupRepo<TS>(List<TS> items) where TS : class, IEntity, new()
        {
            SetupRepo(items?.Count ?? 0, items);
        }
        protected void SetupRepo<TS>(int itemsNumber, List<TS> items = null) where TS : class, IEntity, new()
        {
            items ??= new List<TS>();
            Mock<IRepository<TS>> repoMock = new();
            var mockDbSet = GetQueryableMockDbSet(items);
            repoMock.Setup(r => r.DbSet).Returns(mockDbSet);
            repoMock.Setup(r => r.Queryable).Returns(mockDbSet);
            repoMock.Setup(r => r.GetAll(false)).ReturnsAsync(items.Take(itemsNumber).AsQueryable);
            for (var i = 0; i < items.Count; i++)
            {
                var i1 = items[i].Key;
                repoMock.Setup(r => r.GetOne(i1)).ReturnsAsync(items[i]);

            }

            repoMock.Setup(r => r.Update(It.IsAny<TS>())).Callback<TS>(e => UpdatedEntities.Add((ActionEntity.Update, e))).ReturnsAsync((TS e) => e);
            
            repoMock.Setup(r => r.Add(It.IsAny<TS>())).Callback<TS>((e) =>
            {
                UpdatedEntities.Add((ActionEntity.Add, e));
            }).ReturnsAsync((TS e) => e);
            repoMock.Setup(r => r.Any(It.IsAny<Expression<Func<TS, bool>>>())).Returns((Expression<Func<TS, bool>> a) => Task.FromResult(items.Any(a.Compile())));
            repoMock.Setup(r => r.FindOne(It.IsAny<Expression<Func<TS, bool>>>())).Returns((Expression<Func<TS, bool>> a) => Task.FromResult(items.FirstOrDefault(a.Compile())));
            repoMock.Setup(r => r.FindAll(It.IsAny<Expression<Func<TS, bool>>>())).Returns((Expression<Func<TS, bool>> a) => Task.FromResult(items.Where(a.Compile())));
            
            DatabaseLayerMock.Setup(x => x.Repo<TS>()).Returns(repoMock.Object);
        }
    }
    public enum ActionEntity
    {
        Add,
        Update,
        Delete
    }
}
