namespace IWMM.Settings
{
    public class TraefikIpWhiteListSettings
    {
        public TraefikIpWhiteListSettings()
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
