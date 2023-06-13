using FluentScheduler;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;

namespace IWMM.Core
{
    public class FqdnJob : BaseJob
    {
        private readonly ISettingsToSchemaFacade _settingsToSchemaFacade;

        public FqdnJob(ILogger<FqdnJob> logger, IOptions<MainSettings> options, ISettingsToSchemaFacade settingsToSchemaFacade) : base(logger, options)
        {
            _settingsToSchemaFacade = settingsToSchemaFacade;
        }

        public override void Execute()
        {
            if (Working) return;
            try
            {
                lock (this)
                {
                    Logger.LogInformation(
                        $"Launching Fqdn Discover Job. Each : {Options.Value.FqdnUpdateJobSeconds} second/s.");
                    Working = true;
                    _settingsToSchemaFacade
                        .UpdateEntriesAndSaveIntoRepository(Options.Value.Entries);
                }
            }
            finally
            {
                Logger.LogInformation(
                    $"Stop Discover Job. Each : {Options.Value.FqdnUpdateJobSeconds} second/s.");
                Working = false;
            }
        }
    }
}
