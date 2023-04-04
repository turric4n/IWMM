using IWMM.Entities;
using IWMM.Settings;

namespace IWMM.Services.Abstractions
{
    public interface ISettingsToSchemaFacade
    {
        public EntryBook GenerateEntryBook(IpWhiteListSettings whitelistSetting,
            List<ExcludedEntries> excludedEntriesSetting, List<Group> entryGroups, List<Settings.Entry> entries);
        public void UpdateEntriesAndSaveIntoRepository(List<Settings.Entry> entries);
        public ISchemaRepository GetSchemaRepository(SchemaType schema);
        public IEntriesToSchemaAdaptor GetSchemaAdaptor(SchemaType schema);

    }
}
