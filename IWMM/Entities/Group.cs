using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWMM.Entities
{
    public class Group
    {
        public Group()
        {
            Entries = new List<string>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Entries { get; set; }
    }
}