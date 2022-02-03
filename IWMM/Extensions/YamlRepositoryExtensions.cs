using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class YamlRepositoryExtensions
    {
        public static IServiceCollection UseYamlSerializerDeserializer(this IServiceCollection serviceCollection)
        {
            var yamlDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlSerializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            serviceCollection.TryAddSingleton<IDeserializer>(yamlDeserializer);
            serviceCollection.TryAddSingleton<ISerializer>(yamlSerializer);

            return serviceCollection;
        }
    }
}
