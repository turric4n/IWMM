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
            PlainIps = new List<string>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Fqdn { get; set; }
        public string Groups { get; set; }
        public List<string> PlainIps { get; set; }
        public string CurrentIp { get; set; }
        public string PreviousIp { get; set; }
    }
}
