namespace IWMM.Settings
{
    public class OpnSenseIpWhiteListSettings
    {
        public OpnSenseIpWhiteListSettings()
        {

            AllowedEntries = new List<string>();
            ExcludedEntries = new List<string>();
        }

        public SchemaType SchemaType { get; set; }
        public List<string> AllowedEntries { get; set; }
        public List<string> ExcludedEntries { get; set; }
    }
}
