using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MorkoBotRavenEdition.Services
{
    public class UriInvokerService
    {
        public delegate Task InvokeDelegate(string value);
        private readonly UriTemplateTable _table = new UriTemplateTable(new Uri("bcit://local/"));

        public UriInvokerService()
        {
            RegisterDefaults();
        }

        public async Task InvokeAsync(string path, string value)
        {
            // example path: bcit://local/config/guilds/join <guild id>
        }

        public void Register(string path, InvokeDelegate del)
        {
            _table.KeyValuePairs.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(path), del));
        }

        private void RegisterDefaults()
        {
            Register("config/guilds/admin/warn", val => { return null; });
        }
    }
}
