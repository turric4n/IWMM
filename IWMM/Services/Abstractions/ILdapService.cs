using IWMM.Entities;

namespace IWMM.Services.Abstractions
{
    public interface ILdapService
    {
        public IEnumerable<Entry> RetrieveEntries(string searchFilter);
    }
}
