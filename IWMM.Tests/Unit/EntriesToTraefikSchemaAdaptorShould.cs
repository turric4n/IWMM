using IWMM.Entities;
using IWMM.Services.Impl.Traefik;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IWMM.Tests
{
    [TestFixture(Category = "Unit Test")]
    public class EntriesToTraefikSchemaAdaptorShould
    {
        private EntriesToTraefikSchemaAdaptor traefikSchemaAdaptor;

        [SetUp]
        public void Setup()
        {
            traefikSchemaAdaptor = new EntriesToTraefikSchemaAdaptor();
        }

        [Test]
        public void Given_Valid_Entry_Should_Generate_Valid_Traefik_Dynamic_Object()
        {
            /* TRAEFIK SCHEMA             
                http:
                  middlewares:        
                    allowedips:
                      ipWhiteList:
                        sourceRange:
                          - 1.1.1.1
             */

            //Arrange stubs
            var entry = new Entry() { CurrentIp = "1.1.1.1", Name = "Test" };
            var entry2 = new Entry() { CurrentIp = "2.2.2.2", Name = "Test" };

            var entries = new List<Entry>();

            entries.Add(entry);
            entries.Add(entry2);

            var middlewareName = "testingAllowed";


            //Act
            var schema = traefikSchemaAdaptor.GetSchema(entries, middlewareName);

            //Arrange
            Assert.IsTrue(((IDictionary<string, object>)schema).ContainsKey("http"));
            Assert.IsTrue(((IDictionary<string, object>)((IDictionary<string, object>)schema).Values.First()).ContainsKey("middlewares"));
            Assert.IsTrue(((IDictionary<string, object>)((IDictionary<string, object>)((IDictionary<string, object>)schema).Values.First()).Values.First()).ContainsKey(middlewareName));
            Assert.IsTrue(((IDictionary<string, object>)((IDictionary<string, object>)((IDictionary<string, object>)((IDictionary<string, object>)schema).Values.First()).Values.First()).Values.First()).ContainsKey("ipWhiteList"));
            Assert.IsTrue(((IDictionary<string, object>)((IDictionary<string, object>)((IDictionary<string, object>)((IDictionary<string, object>)((IDictionary<string, object>)schema).Values.First()).Values.First()).Values.First()).Values.First()).ContainsKey("sourceRange"));
            Assert.IsTrue(
                ((List<string>)
                    ((IDictionary<string, object>)
                        ((IDictionary<string, object>)
                            ((IDictionary<string, object>)
                                ((IDictionary<string, object>)((IDictionary<string, object>)schema).Values.First()).Values.First()).Values.First()).Values.First()).Values.First()).Contains("1.1.1.1"));
        }

    }
}