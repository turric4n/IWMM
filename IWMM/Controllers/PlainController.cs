using IWMM.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IWMM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlainController
    {
        private readonly IEntryRepository _entryRepository;

        public PlainController(IEntryRepository entryRepository)
        {
            _entryRepository = entryRepository;
        }

        [HttpGet("ldap/{dn}")]
        public string Index(string dn)
        {
            var targetDn = dn.Replace(';', ',');
            var entries = _entryRepository.FindByDn(targetDn).ToList();
            var result = string.Join("\r\n", entries.Select(entry => entry.CurrentIp));
            return result;
        }
    }
}
