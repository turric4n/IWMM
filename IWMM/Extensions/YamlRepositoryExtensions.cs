using Microsoft.Extensions.DependencyInjection.Extensions;
using YamlDotNet.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class YamlRepositoryExtensions
    {
        public static IServiceCollection UseYamlSerializerDeserializer(this IServiceCollection serviceCollection)
        {
            var yamlDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlSerializer = new YamlDotNet.Serialization.SerializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            serviceCollection.TryAddSingleton<IDeserializer>(yamlDeserializer);
            serviceCollection.TryAddSingleton<ISerializer>(yamlSerializer);

            return serviceCollection;
        }
    }
}
