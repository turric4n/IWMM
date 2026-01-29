using IWMM.Services.Impl.Network;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWMM.Services.Impl.Ldap;
using IWMM.Settings;
using Microsoft.Extensions.Options;

namespace IWMM.Tests.Unit
{
    [TestFixture(Category = "Unit Test")]
    [Explicit("Requires a configured LDAP server to run")]
    public class LdapServiceShould
    {
        private Mock<ILogger<LdapService>> loggerMock;
        private Mock<IOptions<MainSettings>> mainSettingsMock;

        [SetUp]
        public void Setup()
        {
            loggerMock = new Mock<ILogger<LdapService>>();
            mainSettingsMock = new Mock<IOptions<MainSettings>>();
            mainSettingsMock.Setup(x => x.Value).Returns(new MainSettings()
            {
                BaseLdapUri = "ldap://*:389/",
                LdapScavengeScope = "*",
            });
        }

        [Test]
        public void Return_entries_from_ldap_server_given_valid_dsn()
        {
            
            var ldapService = new LdapService(mainSettingsMock.Object, loggerMock.Object);
            var entries = ldapService.RetrieveEntries("(objectClass=Computer)");
            Assert.IsNotNull(entries);
            Assert.IsNotEmpty(entries);
            Assert.IsTrue(entries.All(x => !string.IsNullOrWhiteSpace(x.Dn)));
            Assert.IsTrue(entries.All(x => !string.IsNullOrWhiteSpace(x.Name)));
            Assert.IsTrue(entries.Count() > 10);
        }
    }
}
