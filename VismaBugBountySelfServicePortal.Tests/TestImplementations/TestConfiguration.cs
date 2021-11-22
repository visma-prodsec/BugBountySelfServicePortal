using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace VismaBugBountySelfServicePortal.Tests.TestImplementations
{
    public class TestConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _values = new();
        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get => _values.ContainsKey(key) ? _values[key] : string.Empty;
            set => _values[key] = value;
        }
    }
}
