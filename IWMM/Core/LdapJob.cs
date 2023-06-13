using FluentScheduler;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;

namespace IWMM.Core
{
    public class LdapJob : BaseJob
    {
        private readonly ISettingsToSchemaFacade _settingsToSchemaFacade;

        public LdapJob(ILogger<LdapJob> logger, IOptions<MainSettings> options, ISettingsToSchemaFacade settingsToSchemaFacade) : base(logger, options)
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
                        $"Launching Ldap Discover Job. Each : {Options.Value.LdapUpdateJobSeconds} second/s.");
                    Working = true;
                    _settingsToSchemaFacade.UpdateLdapEntriesAndSaveIntoRepository();
                        
                }
            }
            finally
            {
                Logger.LogInformation(
                    $"Stop Discover Job. Each : {Options.Value.LdapUpdateJobSeconds} second/s.");
                Working = false;
            }
        }
    }
}
