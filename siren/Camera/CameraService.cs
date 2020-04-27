using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Handlers;
using MMALSharp.Native;

namespace CatSiren
{
    public class CameraService : DisposableObject, ICameraService
    {
        private MMALCamera camera;
        private readonly SirenSettings settings;
        private readonly ILogger<CameraService> logger;

        public CameraService(
            SirenSettings settings,
            ILogger<CameraService> logger,
            ILoggerFactory loggerFactory)
        {
            this.settings = settings;
            this.logger = logger;

            MMALLog.LoggerFactory = loggerFactory;
        }

        public async Task CaptureFootageAsync()
        {
            this.CleanupOldCaptures();

            this.LazyInitializeCamera();
            TimeSpan duration = this.CaptureDuration;
            string path = CreateCaptureDirectory();
            this.logger.LogInformation("Capturing footage to {path} for {duration}", path, duration);

            using (var imgCaptureHandler = CreateImageStreamHandler(path))
            using (var cts = new CancellationTokenSource(duration))
            {
                await this.camera.TakePictureTimeout(
                    imgCaptureHandler,
                    MMALEncoding.JPEG,
                    MMALEncoding.I420,
                    cts.Token);
            }
        }

        protected override void DisposeManagedResources()
        {
            this.logger.LogInformation("Releasing camera resources");
            this.camera?.Cleanup();
        }

        private void LazyInitializeCamera()
        {
            if (this.camera == null)
            {
                this.InitializeCamera();
            }
        }

        private void InitializeCamera()
        {
            this.camera = MMALCamera.Instance;

            // Attempt to prevent blurry photos taking during motion by adjusting shutter speed
            MMALCameraConfig.ExposureCompensation
                = (int)MMAL_PARAM_EXPOSUREMODE_T.MMAL_PARAM_EXPOSUREMODE_SPORTS;

            if (!Directory.Exists(this.settings.MediaPath))
                Directory.CreateDirectory(this.settings.MediaPath);
        }

        private TimeSpan CaptureDuration => TimeSpan.FromSeconds(this.settings.CaptureDuration);

        private ImageStreamCaptureHandler CreateImageStreamHandler(string capturePath)
        {
            return new ImageStreamCaptureHandler(capturePath, "jpg");
        }

        private string CreateCaptureDirectory()
        {
            string path = Path.Combine(
                this.settings.MediaPath,
                DateTime.Now.ToString("dd-MMM-yy HH-mm-ss"));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private void CleanupOldCaptures()
        {
            var mediaPath = new DirectoryInfo(this.settings.MediaPath);
            var cutoff = DateTime.UtcNow.Subtract(TimeSpan.FromDays(this.settings.MediaCleanupAfterDays));
            var dirs = mediaPath.GetDirectories().Where(d => d.CreationTimeUtc < cutoff);

            foreach (var dir in dirs)
            {
                this.logger.LogInformation("Deleted old footage in {path}", dir.Name);
                Directory.Delete(dir.FullName, true);
            }
        }
    }
}