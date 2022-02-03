using System.Threading.Tasks;
using IWMM.Services.Abstractions;
using IWMM.Services.Impl.Network;
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
            fqdnResolver = new FqdnResolver();
        }

        [TestCase("test.com", "67.225.146.248")]
        [TestCase("www.pisos.com", "80.94.3.90")]
        [TestCase("fotocasa.es", "52.211.156.62")]
        public async Task Given_Valid_Fqdn_String_Return_Ip_Address(string fqdn, string expectedIp)
        {
            //Act
            var result = await fqdnResolver.GetIpAddressAsync(fqdn);
            //Assert
            Assert.AreEqual(expectedIp, result);
        }

        [TestCase("test.")]
        [TestCase("www.piso")]
        [TestCase("fotoc")]
        public void Given_Invalid_Fqdn_String_Should_Throw(string fqdn)
        { 
            //Act
            AsyncTestDelegate act = () =>
            {
                return fqdnResolver.GetIpAddressAsync(fqdn);
            };

            //Assert

            Assert.ThrowsAsync<FqdnResolverException>(act);
        }
    }
}