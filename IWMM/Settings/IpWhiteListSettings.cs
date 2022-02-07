namespace IWMM.Settings
{
    public class IpWhiteListSettings
    {
        public IpWhiteListSettings()
        {
            AllowedEntries = new List<string>();
            ExcludedEntries = new List<string>();
            TraefikMiddlewareSettings = new TraefikMiddlewareSettings();
        }

        public SchemaType SchemaType { get; set; }
        public TraefikMiddlewareSettings TraefikMiddlewareSettings { get; set; }
        public List<string> AllowedEntries { get; set; }
        public List<string> ExcludedEntries { get; set; }
    }
}
