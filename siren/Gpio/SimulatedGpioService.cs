using System.Collections.Generic;
using System.Device.Gpio;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace CatSiren
{
    public class SimulatedGpioService : IGpioService
    {
        private Dictionary<int, bool> openPins = new Dictionary<int, bool>();
        private readonly ILogger<SimulatedGpioService> logger;

        public SimulatedGpioService(ILogger<SimulatedGpioService> logger)
        {
            this.logger = logger;
        }

        public void Dispose()
        {
        }

        public void OpenPinInput(int pinNumber)
        {
            if (!this.openPins.ContainsKey(pinNumber))
            {
                this.openPins.Add(pinNumber, true);
            }
            else
            {
                this.openPins[pinNumber] = true;
            }

            this.logger.LogInformation("Pin {pin} opened (simulated) in input mode", pinNumber);
        }

        public bool IsPinOpen(int pinNumber)
        {
            return this.openPins.ContainsKey(pinNumber) && this.openPins[pinNumber] == true;
        }

        public void ClosePin(int pinNumber)
        {
            if (this.openPins.ContainsKey(pinNumber))
            {
                this.openPins[pinNumber] = false;
                this.logger.LogInformation("Pin {pin} closed (simulated)", pinNumber);
            }
        }

        public bool Read(int pinNumber)
        {
            return RandomNumberGenerator.GetInt32(1, 100) <= 10;
        }
    }
}