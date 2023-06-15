using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWMM.Entities
{
    public class Entry
    {
        public Entry()
        {
            AdditionalIps = new List<string>();
            Dn = "undefined";
        }

        public int Id { get; set; }
        public string Dn { get; set; }
        public string Name { get; set; }
        public string Fqdn { get; set; }
        public List<string> AdditionalIps { get; set; }
        public string CurrentIp { get; set; }
        public string PreviousIp { get; set; }
    }
}
