using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;
using IWMM.Entities;
using LdapForNet;

namespace IWMM.Services.Impl.Ldap
{
    public class LdapService : ILdapService, IDisposable
    {
        private readonly ILogger<LdapService> _logger;
        private readonly MainSettings _mainSettings;
        private readonly LdapConnection _ldapConnection;

        public LdapService(IOptions<MainSettings> mainSettingsOptions, ILogger<LdapService> logger)
        {
            _logger = logger;
            _mainSettings = mainSettingsOptions.Value;
            _ldapConnection = new LdapConnection();
        }

        public IEnumerable<Entities.Entry> RetrieveEntries(string searchFilter)
        {
            try
            {
                _ldapConnection.Connect(_mainSettings.BaseLdapUri);
                _ldapConnection.Bind();
                var entries = _ldapConnection.Search(_mainSettings.LdapScavengeScope, searchFilter);
                var mappedEntries = new List<Entities.Entry>();
                foreach (var entry in entries)
                {
                    var mappedEntry = new Entities.Entry();
                    mappedEntry.Dn = entry.Dn;
                    mappedEntry.Name = entry.DirectoryAttributes.FirstOrDefault(x => x.Name == "name")
                        ?.GetValue<string>() ?? string.Empty;
                    mappedEntry.Fqdn = entry.DirectoryAttributes.FirstOrDefault(x => x.Name == "dNSHostName")
                        ?.GetValue<string>() ?? string.Empty;
                    mappedEntries.Add(mappedEntry);
                }

                return mappedEntries;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }


        public void Dispose()
        {
            _ldapConnection.Dispose();
        }
    }
}
