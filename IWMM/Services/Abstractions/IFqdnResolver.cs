using System.Net;

namespace IWMM.Services.Abstractions
{

    public interface IFqdnResolver
    {
        Task<string> GetIpAddressAsync(string fqdn);
    }
}
