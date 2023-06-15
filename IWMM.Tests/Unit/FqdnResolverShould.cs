using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using IWMM.Services.Abstractions;
using IWMM.Services.Impl.Network;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IWMM.Tests
{
    [TestFixture(Category = "Unit Test")]
    public class FqdnResolverShould
    {
        private FqdnResolver fqdnResolver;

        [SetUp]
        public void Setup()
        {
            var logger = new Mock<ILogger<FqdnResolver>>().Object;
            fqdnResolver = new FqdnResolver(logger);
        }

        [TestCase("test.com", "67.225.146.248")]
        public async Task Given_Valid_Fqdn_String_Return_Ip_Address(string fqdn, string expectedIp)
        {
            //Act
            var result = await fqdnResolver.GetIpAddressesAsync(fqdn);
            //Assert
            //Assert list contains expected IP in the list
            Assert.IsTrue(result.Contains(expectedIp));
        }

        [TestCase("test.")]
        [TestCase("www.piso")]
        [TestCase("fotoc")]
        public void Given_Invalid_Fqdn_String_Should_Throw(string fqdn)
        { 
            //Act
            AsyncTestDelegate act = () =>
            {
                return fqdnResolver.GetIpAddressesAsync(fqdn);
            };

            //Assert

            Assert.ThrowsAsync<FqdnResolverException>(act);
        }
    }
}