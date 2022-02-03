using System.Dynamic;
using IWMM.Services.Abstractions;
using YamlDotNet.Serialization;

namespace IWMM.Services.Impl.Traefik
{
    public class TraefikWhitelistYamlRepository : ISchemaRepository
    {
        private readonly string _yamlPath;

        private readonly IDeserializer _yamlDeserializer;

        private readonly ISerializer _yamlSerializer;

        public TraefikWhitelistYamlRepository(
            IDeserializer deserializer, 
            ISerializer serializer)
        {
            _yamlDeserializer = deserializer;

            _yamlSerializer = serializer;
        }

        private dynamic LoadFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new YamlFileNotFoundException($"Traefik file not exists -> {path}");
            }

            ExpandoObject result;

            lock (this)
            {
                result = _yamlDeserializer.Deserialize<ExpandoObject>(File.ReadAllText(path));
            }

            return result;
        }

        public void Save(dynamic schema, string path)
        {
            try
            {
                var content = _yamlSerializer.Serialize(schema);
                File.WriteAllText(path, content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new YamlFileException($"Couldn't create yaml file! -> {e.Message}");
            }
        }

        public dynamic Load(string path)
        {
            return LoadFile(path);
        }

        public string Serialize(dynamic schema)
        {
            return _yamlSerializer.Serialize(schema);
        }
    }
}
