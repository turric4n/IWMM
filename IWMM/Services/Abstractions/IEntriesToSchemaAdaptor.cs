using IWMM.Entities;

namespace IWMM.Services.Abstractions
{
    public interface IEntriesToSchemaAdaptor
    {
        public dynamic GetSchema(IEnumerable<EntryBook> entryBooks);
    }
}
