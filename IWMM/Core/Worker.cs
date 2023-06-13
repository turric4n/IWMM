using FluentScheduler;
using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;

namespace IWMM.Core
{
    public class Worker : IHostedService
    {
        private readonly ISettingsToSchemaFacade _settingsToSchemaFacade;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly Func<JobType, IJob> _jobLocator;
        private readonly IOptions<MainSettings> _optionsSnapshot;
        private readonly ILogger<Worker> _logger;
        private int _fqdnResolveScheduleSeconds;
        private int _ldapScanJobScheduleSeconds;


        public Worker(
            ISettingsToSchemaFacade settingsToSchemaFacade,
            IHostEnvironment hostEnvironment,
            Func<JobType, IJob> jobLocator,
            IOptions<MainSettings> optionsSnapshot,
            ILogger<Worker> logger)
        {
            _settingsToSchemaFacade = settingsToSchemaFacade;
            _hostEnvironment = hostEnvironment;
            _jobLocator = jobLocator;
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service...");

            _logger.LogInformation("Environment: " + _hostEnvironment.EnvironmentName);

            try
            {
                JobManager.Initialize();

                await Task.Run(() =>
                {
                    _fqdnResolveScheduleSeconds = (_optionsSnapshot.Value.FqdnUpdateJobSeconds < 30)
                        ? 30 : _optionsSnapshot.Value.FqdnUpdateJobSeconds;

                    _ldapScanJobScheduleSeconds = (_optionsSnapshot.Value.LdapUpdateJobSeconds < 30)
                        ? 30 : _optionsSnapshot.Value.LdapUpdateJobSeconds;

                    JobManager.AddJob(_jobLocator.Invoke(JobType.Fqdn), schedule =>
                        {
                            schedule.WithName("FqdnUpdate Job Process")
                                .ToRunEvery((int)_fqdnResolveScheduleSeconds)
                                .Seconds();
                            schedule.Execute();
                        });

                    if (_optionsSnapshot.Value.UseLdap)
                    {
                        JobManager.AddJob(_jobLocator.Invoke(JobType.Ldap), schedule =>
                            {
                                schedule.WithName("Ldap Scanner Job Process")
                                    .ToRunEvery((int)_ldapScanJobScheduleSeconds)
                                    .Seconds();
                                schedule.Execute();
                            });
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }

            _logger.LogInformation("Started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            JobManager.Stop();
            JobManager.RemoveAllJobs();

            _logger.LogInformation("Service and jobs are stopped");

            await Task.CompletedTask;
        }
    }
}
