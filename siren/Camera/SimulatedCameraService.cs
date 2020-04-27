using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CatSiren
{
    public class SimulatedCameraService : ICameraService
    {
        private readonly ILogger<SimulatedCameraService> logger;
        private readonly SirenSettings settings;

        public SimulatedCameraService(
            ILogger<SimulatedCameraService> logger,
            SirenSettings settings)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public async Task CaptureFootageAsync()
        {
            this.logger.LogInformation("TODO: capture footage with camera");
            await Task.Delay(TimeSpan.FromSeconds(this.settings.CaptureDuration));
        }

        public void Dispose()
        {
        }
    }
}