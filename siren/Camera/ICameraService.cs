using System;
using System.Threading.Tasks;

namespace CatSiren
{
    public interface ICameraService : IDisposable
    {
         Task CaptureFootageAsync();
    }
}