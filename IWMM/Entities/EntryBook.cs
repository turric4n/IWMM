using IWMM.Settings;

namespace IWMM.Entities
{
    public class EntryBook
    {
        public EntryBook(string middlewareName, IEnumerable<Entry> entries, IEnumerable<ExcludedEntries> middlewareExcludedIps)
        {
            MiddlewareName = middlewareName;
            Entries = entries;
            MiddlewareExcludedEntries = middlewareExcludedIps;
        }

        public string MiddlewareName { get; private set; }
        public IEnumerable<Entry> Entries { get; private set; }
        public IEnumerable<ExcludedEntries> MiddlewareExcludedEntries { get; private set; }
    }
}
