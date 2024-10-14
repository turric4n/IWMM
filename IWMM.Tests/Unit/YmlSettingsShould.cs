using FluentAssertions;
using IWMM.Settings;
using NUnit.Framework;
using System.IO;
using YamlDotNet.Serialization;

namespace IWMM.Tests.Unit
{
    [TestFixture(Category = "Unit Test")]
    public class YmlSettingsShould
    {
        [Test]
        public void Return_valid_yml_settings_given_valid_yml()
        {
            //Arrange
            var ymlDeserializer = new Deserializer();

            var yml = File.ReadAllText("iwmm.yml");

            //Act
            var settings = ymlDeserializer.Deserialize(yml);

            //Assert
            settings.Should().NotBeNull();
        }
    }
}
