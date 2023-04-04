using System.Dynamic;
using IWMM.Entities;
using IWMM.Services.Abstractions;

namespace IWMM.Services.Impl.Traefik
{
    public class EntriesToTraefikSchemaAdaptor : IEntriesToSchemaAdaptor
    {

        public dynamic GetSchema(IEnumerable<EntryBook> entryBooks)
        {
            dynamic schema = new ExpandoObject();

            var header = schema.http = new ExpandoObject() as IDictionary<string, object>;

            var subheader = header.middlewares = new ExpandoObject() as IDictionary<string, object>;

            foreach (var entryBook in entryBooks)
            {
                ((IDictionary<string, object>)subheader).Add(entryBook.MiddlewareName, new ExpandoObject());

                dynamic modulename = ((IDictionary<string, object>)subheader).Last().Value;

                var whitelist = modulename.ipWhiteList = new ExpandoObject() as IDictionary<string, object>;

                //whitelist.sourceRange = new ExpandoObject() as IDictionary<string, object>;

                whitelist.sourceRange = new List<string>();

                foreach (var entry in entryBook.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.CurrentIp))
                    {
                        whitelist.sourceRange.Add(entry.CurrentIp);
                    }

                    foreach (var ip in entry.PlainIps.Where(ip => ip != entry.CurrentIp))
                    {
                        whitelist.sourceRange.Add(ip);
                    }
                }

                if (!entryBook.MiddlewareExcludedEntries.Any()) continue;
                {
                    whitelist.ipStrategy = new ExpandoObject() as IDictionary<string, object>;

                    whitelist.ipStrategy.excludedIPs = new List<string>();
                    foreach (var excludedIp in entryBook.MiddlewareExcludedEntries)
                    {
                        var ips = excludedIp.Ips.Split(',');
                        foreach (var ip in ips)
                        {
                            whitelist.ipStrategy.excludedIPs.Add(ip);
                        }
                    }
                }
            }

            return schema;
        }
    }
}
