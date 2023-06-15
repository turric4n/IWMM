using System.Net;

namespace IWMM.Services.Abstractions
{

    public interface IFqdnResolver
    {
        Task<IEnumerable<string>> GetIpAddressesAsync(string fqdn);
    }
}
