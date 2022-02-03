namespace IWMM.Settings
{
    public class MainSettings
    {
        public List<IpWhiteListSettings> IpWhiteListSettings { get; set; }
        public List<Entry> Entries { get; set; }
        public string DatabasePath { get; set; } = @"iwmm.db";
        public int FqdnUpdateJobSeconds { get; set; }
        public int ExporterJobSeconds { get; set; }
    }
}
