using IWMM.Entities;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Entry = IWMM.Entities.Entry;

namespace IWMM.Services.Impl.Facade
{
    public class SettingsToSchemaFacade : ISettingsToSchemaFacade
    {
        private readonly ILogger<SettingsToSchemaFacade> _logger;
        private readonly IFqdnResolver _fqdnResolver;
        private readonly Func<SchemaType, ISchemaRepository> _schemaRepositoryLocator;
        private readonly Func<SchemaType, IEntriesToSchemaAdaptor> _schemaAdaptorLocator;
        private readonly ILdapService _ldapService;
        private readonly IEntryRepository _entryRepository;

        public SettingsToSchemaFacade(IHostEnvironment hostEnvironment,
            ILogger<SettingsToSchemaFacade> logger,
            IFqdnResolver fqdnResolver,
            Func<SchemaType, ISchemaRepository> schemaRepositoryLocator,
            Func<SchemaType, IEntriesToSchemaAdaptor> schemaAdaptorLocator,
            ILdapService ldapService,
            IEntryRepository entryRepository)
        {
            _logger = logger;
            _fqdnResolver = fqdnResolver;
            _schemaRepositoryLocator = schemaRepositoryLocator;
            _schemaAdaptorLocator = schemaAdaptorLocator;
            _ldapService = ldapService;
            _entryRepository = entryRepository;
        }

        private List<string> MergeAllowedEntries(TraefikIpWhiteListSettings whitelistSetting, List<Group> groups, List<Settings.Entry> entries)
        {
            var whitelistGroupEntryNames = groups
                .Where(group => whitelistSetting.AllowedEntries.Contains(group.Name))
            .SelectMany(x => x.Entries)
            .ToList();

            var whitelistSingleEntryNames = entries
                .Where(singleEntry => whitelistSetting.AllowedEntries.Contains(singleEntry.Name) &&
                                      string.IsNullOrEmpty(singleEntry.Groups))
                .Select(x => x.Name)
                .ToList();

            var allAllowedEntries = new List<string>();

            allAllowedEntries = allAllowedEntries
                .Concat(whitelistGroupEntryNames)
                .Concat(whitelistSingleEntryNames)
                .Distinct()
                .ToList();

            return allAllowedEntries;
        }

        public void UpdateEntriesAndSaveIntoRepository(List<Settings.Entry> entries)
        {
            var ownedEntries = entries.Where(entry => string.IsNullOrEmpty(entry.Groups));

            foreach (var entry in ownedEntries)
            {
                try
                {
                    var plainIps = GetResolvedIpsFromEntry(entry);

                    var resolvedIp = string.Empty;
                    if (string.IsNullOrEmpty(entry.Ips) && string.IsNullOrEmpty(entry.Groups))
                    {
                        if (plainIps.Count > 0) resolvedIp = plainIps.First();
                    }

                    var dbEntry = _entryRepository.GetByName(entry.Name);
                    dbEntry.PreviousIp = dbEntry.CurrentIp;
                    dbEntry.CurrentIp = resolvedIp;
                    dbEntry.AdditionalIps = plainIps;
                    dbEntry.Fqdn = entry.Fqdn;
                    dbEntry.Name = entry.Name;
                    dbEntry.Dn = "undefined";

                    _entryRepository.AddOrUpdate(dbEntry);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while processing entry -> {entry.Name} {entry.Fqdn}", e);
                }
            }
        }

        public void UpdateLdapEntriesAndSaveIntoRepository()
        {
            var entries = _ldapService.RetrieveEntries("(objectClass=Computer)");

            foreach (var entry in entries)
            {
                try
                {
                    var plainIps = GetResolvedIpsFromEntry(entry);

                    var dbEntry = _entryRepository.GetByName(entry.Name);
                    dbEntry.PreviousIp = dbEntry.CurrentIp;
                    dbEntry.CurrentIp = plainIps.First();
                    plainIps.Remove(dbEntry.CurrentIp);
                    dbEntry.AdditionalIps = plainIps;
                    dbEntry.Fqdn = entry.Fqdn;
                    dbEntry.Name = entry.Name;
                    dbEntry.Dn = entry.Dn;

                    _entryRepository.AddOrUpdate(dbEntry);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while processing entry -> {entry.Name} {entry.Fqdn}", e);
                }
            }
        }

        public EntryBook GenerateEntryBook(TraefikIpWhiteListSettings whitelistSetting, List<ExcludedEntries> excludedEntriesSetting, List<Group> entryGroups, List<Settings.Entry> entries)
        {
            if (!entries.Any() || whitelistSetting.AllowedEntries.Count < 1) return null;

            var allAllowedEntries = MergeAllowedEntries(whitelistSetting, entryGroups, entries);

            var discoveredEntryEntities = GetEntriesByNames(allAllowedEntries);

            var excludedEntries =
                excludedEntriesSetting
                    .Where(x => whitelistSetting.ExcludedEntries.Any(n =>
                        n.ToLowerInvariant() == x.Name.ToLowerInvariant()));

            var middlewareName = GetOptionalMiddlewareName(whitelistSetting, whitelistSetting.SchemaType);

            var entryBook = new EntryBook(middlewareName, discoveredEntryEntities, excludedEntries);

            return entryBook;
        }

        public ISchemaRepository GetSchemaRepository(SchemaType schema)
        {
            return _schemaRepositoryLocator(schema);
        }

        public IEntriesToSchemaAdaptor GetSchemaAdaptor(SchemaType schema)
        {
            return _schemaAdaptorLocator(schema);
        }

        private IEnumerable<Entry> GetEntriesByNames(IEnumerable<string> list)
        {
            return _entryRepository.FindByNames(list);
        }

        private string ExportWhitelistSchema(IEnumerable<EntryBook> entryBooks,
            IEntriesToSchemaAdaptor schemaAdaptor,
            ISchemaRepository schemaRepository,
            string path = "")
        {
            return schemaAdaptor.GetSchema(entryBooks);
        }

        private string GetOptionalMiddlewareName(TraefikIpWhiteListSettings whiteListSettings, SchemaType schema)
        {
            switch (schema)
            {
                case SchemaType.TraefikIpWhitelistMiddlewareFile:
                    return whiteListSettings.TraefikMiddlewareSettings.Name;
                default:
                    return "";
            }
        }

        private string GetOptionalPath(TraefikIpWhiteListSettings whiteListSettings, SchemaType schema)
        {
            switch (schema)
            {
                case SchemaType.TraefikIpWhitelistMiddlewareFile:
                    return whiteListSettings.TraefikMiddlewareSettings.FilePath;
                default:
                    return "";
            }
        }

        private List<string> GetResolvedFqdn(string fqdn)
        {
            var ips = new List<string>();
            foreach (var entryFqdn in fqdn.Split(','))
            {
                var resolvedIps = _fqdnResolver.GetIpAddressesAsync(entryFqdn).Result;
                ips.AddRange(resolvedIps);
            }
            return ips;
        }

        private List<string> GetResolvedIpsFromEntry(Settings.Entry entry)
        {
            if (!string.IsNullOrEmpty(entry.Groups))
            {
                throw new Exception("Invalid entry!");
            }

            var plainIps = !string.IsNullOrEmpty(entry.Ips) ? entry.Ips.Split(',').ToList() : new List<string>();

            var resolvedFqdn = !string.IsNullOrEmpty(entry.Fqdn) ? GetResolvedFqdn(entry.Fqdn) : new List<string>();

            if (resolvedFqdn.Count != 0)
            {
                plainIps.AddRange(resolvedFqdn);
            }

            return plainIps;
        }

        private List<string> GetResolvedIpsFromEntry(Entry entry)
        {
            var resolvedFqdn = !string.IsNullOrEmpty(entry.Fqdn) ? GetResolvedFqdn(entry.Fqdn) : new List<string>();

            return resolvedFqdn;
        }
    }
}
