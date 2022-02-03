using System.Net;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;

namespace IWMM.Services.Impl.Network
{
    public class FqdnResolver : IFqdnResolver
    {
        private readonly IOptionsSnapshot<MainSettings> _settings;

        public async Task<string> GetIpAddressAsync(string fqdn)
        {
            try
            {
                var gotIps = await Dns.GetHostAddressesAsync(fqdn);

                var gotIp = gotIps
                    .FirstOrDefault()
                    .ToString();

                return gotIp;
            }
            catch (Exception e)
            {
                throw new FqdnResolverException($"Couldn't resolve {fqdn} -> {e.Message}");
            }
        }
    }
}
