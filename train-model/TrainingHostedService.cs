using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatSiren.TrainModel
{
    internal class TrainingHostedService : IHostedService,
        IDisposable
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly CustomVisionTrainingClient _trainingClient;

        public TrainingHostedService(
            ILogger<TrainingHostedService> logger,
            IHostApplicationLifetime appLifetime,
            TrainingOptions trainingOptions)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _trainingClient = CreateClient(trainingOptions);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            //_trainingClient.CreateProjectAsync(

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static CustomVisionTrainingClient CreateClient(TrainingOptions options)
        {
            var trainingApi = new CustomVisionTrainingClient
            {
                ApiKey = options.AzureVision.ApiKey,
                Endpoint = options.AzureVision.Endpoint
            };
            return trainingApi;
        }

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }

        public void Dispose()
        {
            _trainingClient?.Dispose();
        }
    }
}