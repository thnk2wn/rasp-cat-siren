using System;
using System.Device.Gpio;

namespace CatSiren
{
    public class GpioService : DisposableObject, IGpioService
    {
        private GpioController gpio;

        public void OpenPinInput(int pinNumber)
        {
            LazyInitialize(_ => _.OpenPin(pinNumber, PinMode.Input));
        }

        public bool Read(int pinNumber)
        {
            return this.gpio.Read(pinNumber) == true;
        }

        public bool IsPinOpen(int pinNumber)
        {
            return this.gpio.IsPinOpen(pinNumber);
        }

        public void ClosePin(int pinNumber)
        {
            if (IsPinOpen(pinNumber))
            {
                this.gpio.ClosePin(pinNumber);
            }
        }

        protected override void DisposeManagedResources()
        {
            this.gpio?.Dispose();
        }

        private void LazyInitialize(Action<GpioController> action)
        {
            if (this.gpio == null)
            {
                this.gpio = new GpioController();
            }

            action(this.gpio);
        }
    }
}