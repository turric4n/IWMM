using IWMM.Entities;
using IWMM.Repositories;
using IWMM.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Entry = IWMM.Entities.Entry;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IWMM.Tests.Integration
{
    [TestFixture(Category = "Integration Test")]
    public class EntryRepositoryShould
    {
        private LiteDbEntryRepository _entryRepository;

        [SetUp]
        public void Setup()
        {
            var _optionsMock = new Mock<IOptionsSnapshot<MainSettings>>();

            _optionsMock.Setup(m => m.Value)
                .Returns(new MainSettings());

            var _loggerMock = new Mock<ILogger<LiteDbEntryRepository>>()
                .Object;

            _entryRepository = new LiteDbEntryRepository(_loggerMock, _optionsMock.Object);
        }

        [Test]
        public void Given_Valid_Entry_Should_Stored_And_Retrieved_From_Repository()
        {
            //Arrange
            var entry = new Entry();
            entry.Name = "Turrican";
            entry.Fqdn = "www.turric4n.com";
            entry.CurrentIp = "1.1.1.1";
            entry.LatestIp = "2.2.2.2";

            //Act
            _entryRepository.AddOrUpdate(entry);

            var entryRetrieved = _entryRepository.GetByName(entry.Name);
            //Assert

            Assert.IsNotNull(entryRetrieved);
            Assert.AreEqual(entry.Name, entryRetrieved.Name);
            Assert.AreEqual(entry.Fqdn, entryRetrieved.Fqdn);
            Assert.AreEqual(entry.LatestIp, entryRetrieved.LatestIp);
            Assert.AreEqual(entry.CurrentIp, entryRetrieved.CurrentIp);
        }
    }
}
