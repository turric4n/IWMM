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
        private readonly IOptions<MainSettings> _optionsSnapshot;
        private readonly ILogger<Worker> _logger;
        private bool _working;
        private int _currentJobSeconds;


        public Worker(
            ISettingsToSchemaFacade settingsToSchemaFacade,
            IHostEnvironment hostEnvironment,
            IOptions<MainSettings> optionsSnapshot,
            ILogger<Worker> logger)
        {
            _settingsToSchemaFacade = settingsToSchemaFacade;
            _hostEnvironment = hostEnvironment;
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
                    _currentJobSeconds = (_optionsSnapshot.Value.FqdnUpdateJobSeconds < 30)
                        ? 30 : _optionsSnapshot.Value.FqdnUpdateJobSeconds;

                    JobManager.AddJob(
                        () =>
                        {
                            {
                                if (_working) return;
                                try
                                {
                                    lock (this)
                                    {
                                        _logger.LogInformation(
                                            $"Launching Discover Job. Each : {_currentJobSeconds} second/s.");
                                        _working = true;
                                        _settingsToSchemaFacade.UpdateEntriesAndSaveIntoRepository(_optionsSnapshot.Value.Entries);
                                    }
                                }
                                finally
                                {
                                    _logger.LogInformation(
                                        $"Stop Discover Job. Each : {_currentJobSeconds} second/s.");
                                    _working = false;
                                }

                            }
                        }, schedule =>
                        {
                            schedule.WithName("FqdnUpdate Job Process")
                                .ToRunEvery((int)_currentJobSeconds)
                                .Seconds();
                            schedule.Execute();
                        });


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
            JobManager.RemoveAllJobs();

            _logger.LogInformation("Service and jobs are stopped");

            await Task.CompletedTask;
        }
    }
}
