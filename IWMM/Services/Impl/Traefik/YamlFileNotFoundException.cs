namespace IWMM.Services.Impl.Traefik
{
    public class YamlFileNotFoundException : Exception
    {
        public YamlFileNotFoundException(string? message) : base(message)
        {
        }
    }
}
