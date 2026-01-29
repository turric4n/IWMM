using FluentAssertions;
using IWMM.Services.Abstractions;
using IWMM.Services.Impl.Network;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

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

        [TestCase("test.com", "34.224.149.186")]
        public async Task Given_Valid_Fqdn_String_Return_Ip_Address(string fqdn, string expectedIp)
        {
            //Act
            var result = await fqdnResolver.GetIpAddressesAsync(fqdn);
            //Assert
            //Assert list contains expected IP in the list
            result.Should().Contain(expectedIp);
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

        [TestCase("localhost", "127.0.0.1")]
        public async Task Return_expectad_IP_Given_Valid_FQDN(string fqdn, string expectedIp)
        {
            //Act
            var currentIp = await fqdnResolver.GetIpAddressesAsync(fqdn);    

            //Assert            
            currentIp.Should().BeEquivalentTo(expectedIp);
        }
    }
}