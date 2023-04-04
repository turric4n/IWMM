using FluentScheduler;
using IWMM.Entities;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;
using Serilog.Enrichers;
using Entry = IWMM.Entities.Entry;

namespace IWMM.Core
{
    public class Worker : IHostedService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IOptions<MainSettings> _optionsSnapshot;
        private readonly ILogger<Worker> _logger;
        private readonly IFqdnResolver _fqdnResolver;
        private readonly Func<SchemaType, ISchemaRepository> _schemaRepositoryLocator;
        private readonly Func<SchemaType, IEntriesToSchemaAdaptor> _schemaAdaptorLocator;
        private readonly IEntryRepository _entryRepository;
        private bool _working;
        private int _currentJobSeconds;

        public Worker(
            IHostEnvironment hostEnvironment,
            IOptions<MainSettings> optionsSnapshot, 
            ILogger<Worker> logger, 
            IFqdnResolver fqdnResolver,
            Func<SchemaType, ISchemaRepository> schemaRepositoryLocator,
            Func<SchemaType, IEntriesToSchemaAdaptor> schemaAdaptorLocator,
            IEntryRepository entryRepository)
        {
            _hostEnvironment = hostEnvironment;
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
            _fqdnResolver = fqdnResolver;
            _schemaRepositoryLocator = schemaRepositoryLocator;
            _schemaAdaptorLocator = schemaAdaptorLocator;
            _entryRepository = entryRepository;
        }

        private ISchemaRepository GetSchemaRepository(SchemaType schema)
        {
            return _schemaRepositoryLocator(schema);
        }

        private IEntriesToSchemaAdaptor GetSchemaAdaptor(SchemaType schema)
        {
            return _schemaAdaptorLocator(schema);
        }

        private IEnumerable<Entry> GetEntriesByNames(IEnumerable<string> list)
        {
            return _entryRepository.FindByNames(list);
        }

        private void ExportWhitelist(IEnumerable<EntryBook> entryBooks,
            IEntriesToSchemaAdaptor schemaAdaptor,
            ISchemaRepository schemaRepository,
            string path = "")
        {
            var schema = schemaAdaptor.GetSchema(entryBooks);

            if (!string.IsNullOrEmpty(path)) return;

            schemaRepository.Save(schema, path);
        }

        private string GetOptionalMiddlewareName(IpWhiteListSettings whiteListSettings, SchemaType schema)
        {
            switch (schema)
            {
                case SchemaType.TraefikIpWhitelistMiddlewareFile:
                    return whiteListSettings.TraefikMiddlewareSettings.Name;
                default:
                    return "";
            }
        }

        private string GetOptionalPath(IpWhiteListSettings whiteListSettings, SchemaType schema)
        {
            switch (schema)
            {
                case SchemaType.TraefikIpWhitelistMiddlewareFile:
                    return whiteListSettings.TraefikMiddlewareSettings.FilePath;
                default:
                    return "";
            }
        }
        private void ProcessWhitelistSettings()
        {
            var whitelistSettings = _optionsSnapshot.Value.IpWhiteListSettings;

            foreach (var whitelistSetting in whitelistSettings)
            {
                if (whitelistSetting.AllowedEntries.Count < 1) continue;

                var schemaAdaptor = GetSchemaAdaptor(whitelistSetting.SchemaType);

                var schemaRepository = GetSchemaRepository(whitelistSetting.SchemaType);

                var entries = whitelistSetting.AllowedEntries.Count > 0
                    ? GetEntriesByNames(whitelistSetting.AllowedEntries)
                    : new List<Entry>();

                if (!entries.Any()) continue;


                var excludedEntries =
                    _optionsSnapshot
                        .Value
                        .IpStrategyExcludedEntries
                        .Where(x => whitelistSetting.ExcludedEntries.Any(n =>
                            n.ToLowerInvariant() == x.Name.ToLowerInvariant()));


                var middlewareName = GetOptionalMiddlewareName(whitelistSetting, whitelistSetting.SchemaType);

                var middlewarePath = GetOptionalPath(whitelistSetting, whitelistSetting.SchemaType);

                if (string.IsNullOrEmpty(middlewarePath)) continue;

                var entryBook = new EntryBook(middlewareName, entries, excludedEntries);

                var entrybookList = new List<EntryBook>() { entryBook };

                ExportWhitelist(entrybookList, schemaAdaptor, schemaRepository, middlewarePath);
            }
        }

        private List<string> GetResolvedFqdn(string fqdn)
        {
            var ips = new List<string>();
            foreach (var entryFqdn in fqdn.Split(','))
            {
                var resolvedIp = _fqdnResolver.GetIpAddressAsync(entryFqdn).Result;
                ips.Add(resolvedIp);
            }
            return ips;
        }

        private List<string> GetResolvedIpsFromEntry(Settings.Entry entry)
        {
            var plainIps = !string.IsNullOrEmpty(entry.Ips) ? entry.Ips.Split(',').ToList() : new List<string>();

            var resolvedFqdn = !string.IsNullOrEmpty(entry.Fqdn) ? GetResolvedFqdn(entry.Fqdn) : new List<string>();

            plainIps.AddRange(resolvedFqdn);

            if (!string.IsNullOrEmpty(entry.Groups))
            {
                var listGroup = entry.Groups.Split(',');
                foreach (var groupName in listGroup)
                {
                    var gGroup = _optionsSnapshot.Value.Groups.FirstOrDefault(x => groupName.ToLowerInvariant() == x.Name.ToLowerInvariant());
                    foreach (var entryname in gGroup.Entries)
                    {
                        try
                        {
                            var gEntry = _optionsSnapshot.Value.Entries
                                .FirstOrDefault(x => entryname.ToLowerInvariant() == x.Name.ToLowerInvariant());
                            if (gEntry == null) throw new Exception($"Entry {entryname} in Group {gGroup?.Name} not found");
                            plainIps.AddRange(GetResolvedIpsFromEntry(gEntry));
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                        }
                    }
                }
            }
            return plainIps;
        }

        private void UpdateEntries()
        {
            var entries = _optionsSnapshot.Value.Entries;

            foreach (var entry in entries)
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
                    dbEntry.PlainIps = plainIps;
                    dbEntry.Fqdn = entry.Fqdn;
                    dbEntry.Name = entry.Name;

                    _entryRepository.AddOrUpdate(dbEntry);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while processing entry -> {entry.Name} {entry.Fqdn}", e);
                }
            }
        }

        public void FqdnUpdateJob()
        {
            try
            {
                if (_working) return;

                lock (this)
                {
                    _working = true;
                }

                using var scope = (_logger.BeginScope(
                    new Dictionary<string, object>()
                    {
                        {"correlationId", Guid.NewGuid()},
                    }));

                _logger.LogInformation(
                    $"Launching Discover Job. Each : { _currentJobSeconds } second/s.");

                UpdateEntries();

                ProcessWhitelistSettings();

            }
            finally
            {
                _working = false;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service...");

            _logger.LogInformation("Environment: " + _hostEnvironment.EnvironmentName);

            try
            {
                JobManager.Initialize();

                await Task.Run(() =>
                {
                    _currentJobSeconds = (_optionsSnapshot.Value.FqdnUpdateJobSeconds < 30)
                        ? 30 : _optionsSnapshot.Value.FqdnUpdateJobSeconds;

                    JobManager.AddJob(FqdnUpdateJob,
                        s =>
                        {
                            s.WithName("FqdnUpdate Job Process")
                                .ToRunEvery((int)_currentJobSeconds)
                                .Seconds();
                            s.Execute();
                        });

                    //var exporterJobSeconds = (_optionsSnapshot.Value.ExporterJobSeconds < 30)
                    //    ? 30 : _optionsSnapshot.Value.FqdnUpdateJobSeconds;

                    //JobManager.AddJob(FqdnUpdateJob,
                    //    s =>
                    //    {
                    //        s.WithName("Exporter Job Process")
                    //            .ToRunEvery((int)exporterJobSeconds)
                    //            .Seconds();
                    //        s.Execute();
                    //    });

                }, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }

            _logger.LogInformation("Started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            JobManager.RemoveAllJobs();

            _logger.LogInformation("Service and jobs are stopped");

            await Task.CompletedTask;
        }
    }
}
