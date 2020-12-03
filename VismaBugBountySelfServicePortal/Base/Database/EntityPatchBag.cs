using System.Collections.Generic;
using System.Text.Json.Serialization;
using VismaBugBountySelfServicePortal.Models.Entity;

namespace VismaBugBountySelfServicePortal.Base.Database
{
    public class EntityPatchBag<T> where T : IEntity
    {
        public string Id { get; set; }
        public T Model { get; set; }

        public HashSet<string> PropertiesToUpdate { get; set; }

        [JsonIgnore] public bool HasAnything => PropertiesToUpdate != null && PropertiesToUpdate.Count > 0;
    }
}