using System.Net;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IWMM.Services.Impl.Network
{
    public class FqdnResolver : IFqdnResolver
    {
        private readonly ILogger<FqdnResolver> _logger;

        public FqdnResolver(ILogger<FqdnResolver> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetIpAddressAsync(string fqdn)
        {
            try
            {
                var gotIps = await Dns.GetHostAddressesAsync(fqdn);

                var gotIp = gotIps
                    .FirstOrDefault()
                    ?.ToString();

                return gotIp;
            }
            catch (Exception e)
            {
                var message = $"Couldn't resolve {fqdn} -> {e.Message}";

                _logger.LogError(message);

                throw new FqdnResolverException(message);
            }
        }
    }
}
