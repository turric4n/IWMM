using IWMM.Entities;

namespace IWMM.Settings
{
    public class MainSettings
    {
        public MainSettings()
        {
            IpWhiteListSettings = new List<IpWhiteListSettings>();
            Entries = new List<Entry>();
            IpStrategyExcludedEntries = new List<ExcludedEntries>();
            AdditionalTraefikPlainFileSettingsPaths = new List<string>();
        }

        public List<IpWhiteListSettings> IpWhiteListSettings { get; set; }
        public List<string> AdditionalTraefikPlainFileSettingsPaths { get; set; }
        public List<Entry> Entries { get; set; }
        public List<Group> Groups { get; set; }
        public List<ExcludedEntries> IpStrategyExcludedEntries { get; set; }
        public string DatabasePath { get; set; } = @"iwmm.db";
        public int FqdnUpdateJobSeconds { get; set; }
        public int ExporterJobSeconds { get; set; }
    }
}
