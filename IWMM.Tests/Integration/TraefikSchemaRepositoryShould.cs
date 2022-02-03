using System;
using System.Collections.Generic;
using System.IO;
using IWMM.Entities;
using IWMM.Services.Impl.Traefik;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IWMM.Tests.Integration
{
    [TestFixture(Category = "Integration Test")]
    public class TraefikSchemaRepositoryShould
    {
        private TraefikWhitelistYamlRepository _yamelTraefikWhitelistYamlRepository;
        private EntriesToTraefikSchemaAdaptor _traefikSchemaAdaptor;
        private ISerializer _serializer;
        private IDeserializer _deserializer;

        [SetUp]
        public void Setup()
        {
            _deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _yamelTraefikWhitelistYamlRepository = new TraefikWhitelistYamlRepository(_deserializer, _serializer);

            _traefikSchemaAdaptor = new EntriesToTraefikSchemaAdaptor();
        }

             /* TRAEFIK SCHEMA             
                http:
                  middlewares:        
                    allowedips:
                      ipWhiteList:
                        sourceRange:
                          - 1.1.1.1
             */

        private string GetTraefikYaml()
        {
            return string.Join(
                Environment.NewLine,
                "http:",
                "  middlewares:",
                "    testingAllowed:",
                "      ipWhiteList:",
                "        sourceRange:",
                "        - 1.1.1.1",
                "        - 2.2.2.2",
                ""
                );
        }

        private List<Entry> GetDemoEntries()
        {
            //Arrange stubs
            var entry = new Entry() { CurrentIp = "1.1.1.1", Name = "Test" };
            var entry2 = new Entry() { CurrentIp = "2.2.2.2", Name = "Test" };

            var entries = new List<Entry>();

            entries.Add(entry);
            entries.Add(entry2);

            return entries;
        }

        [Test]
        public void Given_Valid_Traefik_Schema_Should_Serialized_Into_Yaml()
        {
            //Arrange
            var middlewareName = "testingAllowed";
            var traefikYaml = GetTraefikYaml();

            var entries = GetDemoEntries();

            //Act
            var schema = _traefikSchemaAdaptor.GetSchema(entries, middlewareName);

            var serializedYaml = _yamelTraefikWhitelistYamlRepository.Serialize(schema);
            
            //Assert
            Assert.AreEqual(traefikYaml, serializedYaml);
        }

        [Test]
        public void Given_Valid_Traefik_Schema_Should_Stored_Into_Disk()
        {
            //Arrange
            var middlewareName = "testingAllowed";

            var settingsPath = "traefik.yml";

            var traefikYaml = GetTraefikYaml();

            var entries = GetDemoEntries();

            //Act
            var schema = _traefikSchemaAdaptor.GetSchema(entries, middlewareName);

            _yamelTraefikWhitelistYamlRepository.Save(schema, settingsPath);

            var content = File.ReadAllText(settingsPath);

            var schemaReloaded = _yamelTraefikWhitelistYamlRepository.Load(settingsPath);

            _yamelTraefikWhitelistYamlRepository.Save(schemaReloaded, settingsPath);

            var contentReloaded = File.ReadAllText(settingsPath);

            //Assert
            Assert.IsTrue(File.Exists(settingsPath));

            Assert.IsTrue(!string.IsNullOrEmpty(content));

            Assert.AreEqual(content, traefikYaml);

            Assert.IsTrue(!string.IsNullOrEmpty(contentReloaded));

            Assert.AreEqual(contentReloaded, traefikYaml);
        }
    }
}
