using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatSiren
{
    public class MotionHost : BackgroundService
    {
        private readonly ILogger<MotionHost> logger;
        private readonly IMotionSiren motionSiren;

        public MotionHost(
            ILogger<MotionHost> logger,
            IMotionSiren motionSiren)
        {
            this.motionSiren = motionSiren;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Motion sensor host is starting up");
            stoppingToken.Register(Shutdown);

            await this.motionSiren.MonitorAsync(stoppingToken);
        }

        private void Shutdown()
        {
            this.logger.LogInformation("Motion sensor hosted service is shutting down");
            this.motionSiren?.Dispose();
        }
    }
}