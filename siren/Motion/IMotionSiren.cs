using System;
using System.Threading;
using System.Threading.Tasks;

namespace CatSiren
{
    public interface IMotionSiren : IDisposable
    {
        Task MonitorAsync(CancellationToken stoppingToken);

        SirenState State { get; }

        bool SensorState { get; }
    }
}