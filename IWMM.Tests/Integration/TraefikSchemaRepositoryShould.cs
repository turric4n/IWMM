using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using IWMM.Entities;
using IWMM.Services.Impl.Traefik;
using IWMM.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Entry = IWMM.Entities.Entry;

namespace IWMM.Tests.Integration
{
    [TestFixture(Category = "Integration Test")]
    public class TraefikSchemaRepositoryShould
    {
        private YamlRepository _yamelYamlRepository;
        private EntriesToTraefikSchemaAdaptor _traefikSchemaAdaptor;
        private ISerializer _serializer;
        private IDeserializer _deserializer;
        private SchemaMerger _schemaMerger;

        [SetUp]
        public void Setup()
        {
            _deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();



            var logger = new Mock<ILogger<YamlRepository>>().Object;

            _yamelYamlRepository = new YamlRepository(_deserializer, _serializer, logger);

            _traefikSchemaAdaptor = new EntriesToTraefikSchemaAdaptor();

            _schemaMerger = new SchemaMerger();


        }

             /* TRAEFIK SCHEMA             
                http:
                  routes:
                  services:
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
        public void Given_Valid_Traefik_Schema_With_Underscore_Should_Serialized_Into_Yaml_Without_CamelCase_Behaviour()
        {
            //Arrange
            var camelcasefilename = "camelcase.yml";
            var validcamelcase = File.ReadAllText("validcamelcase.yml");

            //Act
            var underScoreSchema = _yamelYamlRepository.Load(camelcasefilename);
            string serializedYaml = _yamelYamlRepository.Serialize(underScoreSchema);

            //Assert
            serializedYaml.Should().Be(validcamelcase);
        }

        [Test]
        public void Given_Valid_Traefik_Schema_Should_Serialized_Into_Yaml()
        {
            //Arrange
            var middlewareName = "testingAllowed";
            var traefikYaml = GetTraefikYaml();

            var entries = GetDemoEntries();

            var entryBook = new EntryBook(middlewareName, entries, new List<ExcludedEntries>());

            var listEntrybook = new List<EntryBook>() { entryBook };

            //Act
            var schema = _traefikSchemaAdaptor.GetSchema(listEntrybook);

            string serializedYaml = _yamelYamlRepository.Serialize(schema);

            //Assert
            serializedYaml.Should().Be(traefikYaml);
        }

        [Test]
        public void Return_Valid_Schema_Given_Multiple_Yaml_Files()
        {
            //Arrange
            var firstSchema = "first.yml";
            var secondSchema = "second.yml";
            var validMerge = File.ReadAllText("validmerged.yml");

            //Act
            var firstLoadedSchema = _yamelYamlRepository.Load(firstSchema);
            var secondLoadedSchema = _yamelYamlRepository.Load(secondSchema);
            var loadedSchemaList = new List<dynamic>();
            loadedSchemaList.Add(firstLoadedSchema);
            loadedSchemaList.Add(secondLoadedSchema);
            var mergedSchema = _schemaMerger.Merge(loadedSchemaList);

            //Assert
            string serializedYaml = _yamelYamlRepository.Serialize(mergedSchema);
            serializedYaml.Should().Be(validMerge);
        }

        [Test]
        public void Return_Valid_Schema_Given_Multiple_Yaml_Files_With_Security_Entries()
        {
            //Arrange
            var firstSchema = "first.yml";
            var secondSchema = "second.yml";
            var validMerge = File.ReadAllText("validmerged_with_middlewares.yml");
            var middlewareName = "testingAllowed";
            var settingsPath = "traefik.yml";
            var traefikYaml = GetTraefikYaml();
            var entries = GetDemoEntries();
            var entryBook = new EntryBook(middlewareName, entries, new List<ExcludedEntries>());
            var listEntrybook = new List<EntryBook>() { entryBook };


            //Act
            var firstLoadedSchema = _yamelYamlRepository.Load(firstSchema);
            var secondLoadedSchema = _yamelYamlRepository.Load(secondSchema);
            var thirdLoadedSchema = _traefikSchemaAdaptor.GetSchema(listEntrybook);

            var loadedSchemaList = new List<dynamic>();
            loadedSchemaList.Add(firstLoadedSchema);
            loadedSchemaList.Add(secondLoadedSchema);
            loadedSchemaList.Add(thirdLoadedSchema);

            var mergedSchema = _schemaMerger.Merge(loadedSchemaList);

            //Assert
            string serializedYaml = _yamelYamlRepository.Serialize(mergedSchema);
            serializedYaml.Should().Be(validMerge);
        }


        [Test]
        public void Given_Valid_Traefik_Schema_Should_Stored_Into_Disk()
        {
            //Arrange
            var middlewareName = "testingAllowed";

            var settingsPath = "traefik.yml";

            var traefikYaml = GetTraefikYaml();

            var entries = GetDemoEntries();

            var entryBook = new EntryBook(middlewareName, entries, new List<ExcludedEntries>());

            var listEntrybook = new List<EntryBook>() { entryBook };

            //Act
            var schema = _traefikSchemaAdaptor.GetSchema(listEntrybook);

            _yamelYamlRepository.Save(schema, settingsPath);

            var content = File.ReadAllText(settingsPath);

            var schemaReloaded = _yamelYamlRepository.Load(settingsPath);

            _yamelYamlRepository.Save(schemaReloaded, settingsPath);

            var contentReloaded = File.ReadAllText(settingsPath);

            //Assert
            File.Exists(settingsPath).Should().BeTrue();

            content.Should().NotBeNullOrEmpty();
            content.Should().Be(traefikYaml);

            contentReloaded.Should().NotBeNullOrEmpty();
            contentReloaded.Should().Be(traefikYaml);
        }
    }
}
