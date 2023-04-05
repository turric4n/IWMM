using System.Net;
using System.Net.Sockets;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IWMM.Services.Impl.Network
{
    public class FqdnResolver : IFqdnResolver
    {
        private const string FqdnResolverExceptionMessage = "Couldn't resolve FQDN : ";
        private const string IpV6SubnetPrefix = "::ffff:";


        private readonly ILogger<FqdnResolver> _logger;


        public FqdnResolver(ILogger<FqdnResolver> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetIpAddressAsync(string fqdn)
        {
            try
            {
                var gotIps = await Dns.GetHostAddressesAsync(fqdn, AddressFamily.InterNetwork);

                var gotIp = gotIps
                    .FirstOrDefault()
                    ?.ToString();

                if (gotIp == null)
                {
                    gotIps = await Dns.GetHostAddressesAsync(fqdn, AddressFamily.InterNetworkV6);
                    gotIp = gotIps
                        .FirstOrDefault()
                        ?.ToString();
                }

                if (gotIp.Contains(IpV6SubnetPrefix))
                {
                    gotIp = gotIp.Replace(IpV6SubnetPrefix, string.Empty);
                }

                return gotIp;
            }
            catch (Exception e)
            {
                var message = $"{FqdnResolverExceptionMessage} {fqdn} -> {e.Message}";

                _logger.LogError(message);

                throw new FqdnResolverException(message);
            }
        }
    }
}
