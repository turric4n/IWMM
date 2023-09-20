using IWMM.Entities;
using IWMM.Settings;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Entry = IWMM.Entities.Entry;

namespace IWMM.Repositories
{
    public class LiteDbEntryRepository : IEntryRepository, IDisposable
    {
        private readonly ILogger<LiteDbEntryRepository> _logger;
        private readonly IOptions<MainSettings> _settings;
        private LiteDatabase _liteDatabase;
        private ILiteCollection<Entry> _entries;

        private void CreateDbAndCollection()
        {
            _liteDatabase = new LiteDatabase(_settings.Value.DatabasePath);
            _entries = _liteDatabase.GetCollection<Entry>("Entries");
        }

        public LiteDbEntryRepository(ILogger<LiteDbEntryRepository> logger, 
            IOptions<MainSettings> settings)
        {
            _logger = logger;
            _settings = settings;

            CreateDbAndCollection();
        }

        public void AddOrUpdate(Entry entry)
        {
            var changedIp = entry.CurrentIp != entry.PreviousIp;

            if (entry.IpChanged)
            {
                _logger.LogInformation($"Entry IP changed -> {entry.Name}. CurrentIP : {entry.CurrentIp} - PreviousIP : {entry.PreviousIp}");
            }

            if (!_entries.Update(entry))
            {
                _entries.Insert(entry);
                _logger.LogInformation($"Inserted entry into database -> { entry.Name }. CurrentIP : { entry.CurrentIp } - PreviousIP : { entry.PreviousIp }");
            }
            else
            {
                _logger.LogInformation($"Updated entry into database -> {entry.Name}. CurrentIP : {entry.CurrentIp} - PreviousIP : {entry.PreviousIp } ");
            }            

            _entries.EnsureIndex(x => x.Name);
        }

        public IEnumerable<Entry> GetAll()
        {
            return _entries.FindAll();
        }

        public Entry GetByName(string name)
        {
            return _entries.Find(x => x.Name == name).FirstOrDefault(new Entry());
        }

        public IEnumerable<Entry> FindByNames(IEnumerable<string> names)
        {
            return _entries.FindAll().Where(x => names.Any(n => n.ToLowerInvariant() == x.Name.ToLowerInvariant()));
            //return _entries.Find(x => names.Any(n => n.ToLowerInvariant() == x.Name.ToLowerInvariant()));
        }

        public IEnumerable<Entry> FindByDn(string dn)
        {
            return _entries.FindAll().Where(entry => entry.Dn.ToLowerInvariant().Contains(dn.ToLowerInvariant()));
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
        }
    }
}
