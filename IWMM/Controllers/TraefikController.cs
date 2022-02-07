using IWMM.Entities;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuickLogger.NetStandard;
using YamlDotNet.Serialization;
using Entry = IWMM.Entities.Entry;

namespace IWMM.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TraefikController : Controller
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IOptionsSnapshot<MainSettings> _optionsSnapshot;
        private readonly ILogger<TraefikController> _logger;
        private readonly Func<SchemaType, ISchemaRepository> _schemaRepositoryLocator;
        private readonly Func<SchemaType, IEntriesToSchemaAdaptor> _schemaAdaptorLocator;
        private readonly IEntryRepository _entryRepository;
        private readonly ISerializer _serializer;

        public TraefikController(IHostEnvironment hostEnvironment,
            IOptionsSnapshot<MainSettings> optionsSnapshot,
            ILogger<TraefikController> logger,
            IFqdnResolver fqdnResolver,
            Func<SchemaType, ISchemaRepository> schemaRepositoryLocator,
            Func<SchemaType, IEntriesToSchemaAdaptor> schemaAdaptorLocator,
            IEntryRepository entryRepository, ISerializer serializer)
        {
            _hostEnvironment = hostEnvironment;
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
            _schemaRepositoryLocator = schemaRepositoryLocator;
            _schemaAdaptorLocator = schemaAdaptorLocator;
            _entryRepository = entryRepository;
            _serializer = serializer;
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

        [HttpGet("")]
        public IActionResult Index()
        {

            var whitelistSettings = _optionsSnapshot.Value.IpWhiteListSettings;

            if (whitelistSettings.Count == 0) return Ok("");

            var entryBooks = new List<EntryBook>();

            var schemaAdaptor = GetSchemaAdaptor(SchemaType.TraefikIpWhitelistMiddlewareFile);

            foreach (var whitelistSetting in whitelistSettings)
            {
                if (whitelistSetting.AllowedEntries.Count == 0) break;

                var entries = whitelistSetting.AllowedEntries.Count > 0
                    ? GetEntriesByNames(whitelistSetting.AllowedEntries)
                    : new List<Entry>();

                var excludedEntries =
                    _optionsSnapshot
                        .Value
                        .IpStrategyExcludedEntries
                        .Where(x => whitelistSetting.ExcludedEntries.Any(n =>
                            n.ToLowerInvariant() == x.Name.ToLowerInvariant()));

                var middlewareName = GetOptionalMiddlewareName(whitelistSetting, whitelistSetting.SchemaType);

                var entryBook = new EntryBook(middlewareName, entries, excludedEntries);

                entryBooks.Add(entryBook);
            }

            var schema = schemaAdaptor.GetSchema(entryBooks);

            var schemaYaml = _serializer.Serialize(schema).ToString();

            return Ok(schemaYaml);
        }
    }
}
