using IWMM.Entities;

namespace IWMM.Settings
{
    public class MainSettings
    {
        public MainSettings()
        {
            TraefikIpWhiteListSettings = new List<TraefikIpWhiteListSettings>();
            OpnSenseIpWhiteListSettings = new List<OpnSenseIpWhiteListSettings>();
            Entries = new List<Entry>();
            IpStrategyExcludedEntries = new List<ExcludedEntries>();
            AdditionalTraefikPlainFileSettingsPaths = new List<string>();
        }

        public List<OpnSenseIpWhiteListSettings> OpnSenseIpWhiteListSettings { get; set; }
        public List<TraefikIpWhiteListSettings> TraefikIpWhiteListSettings { get; set; }
        public List<string> AdditionalTraefikPlainFileSettingsPaths { get; set; }
        public List<Entry> Entries { get; set; }
        public List<Group> Groups { get; set; }
        public List<ExcludedEntries> IpStrategyExcludedEntries { get; set; }
        public string DatabasePath { get; set; } = @"iwmm.db";
        public int FqdnUpdateJobSeconds { get; set; } = 30;
        public bool UseLdap { get; set; }
        public string BaseLdapUri { get; set; }
        public string LdapScavengeScope { get; set; }
        public int LdapUpdateJobSeconds { get; set; } = 30;
    }


}
