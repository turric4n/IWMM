using System.Dynamic;
using IWMM.Entities;
using IWMM.Services.Abstractions;

namespace IWMM.Services.Impl.Traefik
{
    public class EntriesToTraefikSchemaAdaptor : IEntriesToSchemaAdaptor
    {
        public dynamic GetSchema(IEnumerable<Entry> entries, string middlewareName = "")
        {
            dynamic schema = new ExpandoObject();

            var header = schema.http = new ExpandoObject() as IDictionary<string, object>;

            var subheader = header.middlewares = new ExpandoObject() as IDictionary<string, object>;

            ((IDictionary<string, object>)subheader).Add(middlewareName, new ExpandoObject());

            dynamic modulename = ((IDictionary<string, object>)subheader).Last().Value;

            var whitelist = modulename.ipWhiteList = new ExpandoObject() as IDictionary<string, object>;

            //whitelist.sourceRange = new ExpandoObject() as IDictionary<string, object>;

            whitelist.sourceRange = new List<string>();

            foreach (var entry in entries)
            {
                whitelist.sourceRange.Add(entry.CurrentIp);
            }

            return schema;
        }
    }
}
