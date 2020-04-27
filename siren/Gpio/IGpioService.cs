using System;
using System.Device.Gpio;

namespace CatSiren
{
    public interface IGpioService : IDisposable
    {
         void OpenPinInput(int pinNumber);

         bool Read(int pinNumber);

         bool IsPinOpen(int pinNumber);

         void ClosePin(int pinNumber);
    }
}