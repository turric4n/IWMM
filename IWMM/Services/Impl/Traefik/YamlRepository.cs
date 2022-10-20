using System.Dynamic;
using IWMM.Services.Abstractions;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace IWMM.Services.Impl.Traefik
{
    public class YamlRepository : ISchemaRepository
    {
        private readonly string _yamlPath;

        private readonly IDeserializer _yamlDeserializer;

        private readonly ISerializer _yamlSerializer;

        private readonly ILogger<YamlRepository> _logger;

        public YamlRepository(
            IDeserializer deserializer, 
            ISerializer serializer, ILogger<YamlRepository> logger)
        {
            _yamlDeserializer = deserializer;

            _yamlSerializer = serializer;
            _logger = logger;
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

                if (path == null)
                    throw new ArgumentNullException(nameof(path));
                if (path.Length == 0)
                    throw new ArgumentException("Empty path!", nameof(path));

                using var sw = new StreamWriter(path);

                _logger.LogInformation($"Store schema into file -> {path}");

                sw.Write(content);
            }
            catch (Exception e)
            {
                var message = $"Couldn't create yaml file! -> {e.Message}";

                _logger.LogCritical(message);

                throw new YamlFileException(message);
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
