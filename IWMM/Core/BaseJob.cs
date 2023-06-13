using FluentScheduler;
using IWMM.Settings;
using Microsoft.Extensions.Options;

namespace IWMM.Core
{
    public abstract class BaseJob : IJob
    {
        protected readonly ILogger Logger;
        protected readonly IOptions<MainSettings> Options;
        protected bool Working;

        protected BaseJob(ILogger logger, IOptions<MainSettings> options)
        {
            Logger = logger;
            Options = options;
            Working = false;
        }

        public abstract void Execute();
    }
}
