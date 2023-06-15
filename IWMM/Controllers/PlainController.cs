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
        public string GetLdapOu(string dn)
        {
            var targetDn = dn.Replace(';', ',');

            var entriesUnfiltered = _entryRepository
                .FindByDn(targetDn)
                .ToList();

            var currentIpEntries = entriesUnfiltered
                .Select(entry => entry.CurrentIp)
                .ToList();

            var additionIpEntries = entriesUnfiltered
                .Select(entry => entry.AdditionalIps)
                .ToList();

            var allIpEntries = currentIpEntries
                .Concat(additionIpEntries.SelectMany(x => x))
                .Distinct()
                .ToList();


            var result = string.Join("\r\n", allIpEntries);

            return result;
        }

        [HttpGet("ldapComputer/{computerName}")]
        public string GetLdapComputer(string computerName)
        {

            var entriesUnfiltered = _entryRepository
                .FindByNames(new []{computerName})
                .ToList();

            var currentIpEntries = entriesUnfiltered
                .Select(entry => entry.CurrentIp)
                .ToList();

            var additionIpEntries = entriesUnfiltered
                .Select(entry => entry.AdditionalIps)
                .ToList();

            var allIpEntries = currentIpEntries
                .Concat(additionIpEntries.SelectMany(x => x))
                .Distinct()
                .ToList();

            var result = string.Join("\r\n", allIpEntries);

            return result;
        }
    }
}
