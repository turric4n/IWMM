using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IOptionsSnapshot<MainSettings> _settings;
        private LiteDatabase _liteDatabase;
        private ILiteCollection<Entry> _entries;

        private void CreateDbAndCollection()
        {
            _liteDatabase = new LiteDatabase(_settings.Value.DatabasePath);
            _entries = _liteDatabase.GetCollection<Entry>("Entries");
        }

        public LiteDbEntryRepository(ILogger<LiteDbEntryRepository> logger, 
            IOptionsSnapshot<MainSettings> settings)
        {
            _logger = logger;
            _settings = settings;

            CreateDbAndCollection();
        }

        public void AddOrUpdate(Entry entry)
        {
            if (!_entries.Update(entry))
            {
                _entries.Insert(entry);
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
            return _entries.Find(x => names.Any(n => n.ToLowerInvariant() == x.Name.ToLowerInvariant()));
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
        }
    }
}
