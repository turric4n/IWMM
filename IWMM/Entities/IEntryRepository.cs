using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWMM.Entities
{
    public interface IEntryRepository
    {
        void AddOrUpdate(Entry entry);
        IEnumerable<Entry> GetAll();
        Entry GetByName(string name);
        IEnumerable<Entry> FindByNames(IEnumerable<string> names);
        IEnumerable<Entry> FindByDn(string dn);
    }
}
