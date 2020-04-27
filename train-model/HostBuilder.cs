using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CatSiren.TrainModel
{
    internal class HostBuilder
    {
        private const string EnvName = "local";

        public IHostBuilder Create(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<TrainingOptions>(hostContext.Configuration);

                    services.AddSingleton(sp => 
                        sp.GetRequiredService<IOptions<TrainingOptions>>().Value);

                    services.AddHostedService<TrainingHostedService>();
                })
                .ConfigureAppConfiguration(configBuilder => 
                {
                    configBuilder
                        .SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{EnvName}.json", optional: true)
                        .AddEnvironmentVariables();
                });
        }
    }
}