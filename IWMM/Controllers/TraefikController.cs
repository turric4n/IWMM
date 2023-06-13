using IWMM.Entities;
using IWMM.Services.Abstractions;
using IWMM.Services.Impl.Facade;
using IWMM.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using Entry = IWMM.Entities.Entry;

namespace IWMM.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TraefikController : Controller
    {
        private readonly ISettingsToSchemaFacade _settingsToSchemaFacade;
        private readonly IOptionsSnapshot<MainSettings> _optionsSnapshot;
        private readonly ILogger<TraefikController> _logger;
        private readonly Func<SchemaType, ISchemaRepository> _schemaRepositoryLocator;
        private readonly Func<SchemaType, IEntriesToSchemaAdaptor> _schemaAdaptorLocator;
        private readonly IEntryRepository _entryRepository;
        private readonly ISchemaMerger _schemaMerger;

        public TraefikController(IHostEnvironment hostEnvironment,
            ISettingsToSchemaFacade settingsToSchemaFacade,
            IOptionsSnapshot<MainSettings> optionsSnapshot,
            ILogger<TraefikController> logger,
            ISchemaMerger schemaMerger)
        {
            _settingsToSchemaFacade = settingsToSchemaFacade;
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
            _schemaMerger = schemaMerger;
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

        [HttpGet("")]
        public IActionResult Index()
        {

            var whitelistSettings = _optionsSnapshot.Value.TraefikIpWhiteListSettings;

            var excludedSettings = _optionsSnapshot.Value.IpStrategyExcludedEntries;

            var groups = _optionsSnapshot.Value.Groups;

            var entries = _optionsSnapshot.Value.Entries;

            var entryBooks = new List<EntryBook>();

            var schemaAdaptor = _settingsToSchemaFacade.GetSchemaAdaptor(SchemaType.TraefikIpWhitelistMiddlewareFile);

            foreach (var whitelistSetting in whitelistSettings)
            {
                entryBooks.Add(_settingsToSchemaFacade.GenerateEntryBook(whitelistSetting, excludedSettings, groups, entries));
            }

            var additionalFileSchemas = GetAdditionalFileSchemas();

            if (entryBooks.Count > 0)
            {
                var schema = schemaAdaptor.GetSchema(entryBooks);

                additionalFileSchemas.Add(schema);
            }

            var merged = _schemaMerger.Merge(additionalFileSchemas);

            var repository = _settingsToSchemaFacade.GetSchemaRepository(SchemaType.TraefikIpWhitelistMiddlewareFile);

            var schemaYaml = repository.Serialize(merged).ToString();

            return Ok(schemaYaml);
        }

        private List<dynamic> GetAdditionalFileSchemas()
        {
            var result = new List<dynamic>();

            var repository = _settingsToSchemaFacade.GetSchemaRepository(SchemaType.TraefikPlain);

            foreach (var additionalSettingsPath in _optionsSnapshot.Value.AdditionalTraefikPlainFileSettingsPaths)
            {
                try
                {
                    var files = Directory.GetFiles(additionalSettingsPath);

                    result.AddRange(files.Select(file => repository.Load(file)));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while loading additional config file -> {additionalSettingsPath}", ex.Message);
                }
            }

            return result;
        }
    }
}
