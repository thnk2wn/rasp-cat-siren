using System;
using System.Device.Gpio;
using System.Threading;

namespace MotionSensorTest
{
    class Program
    {
        private const int PIR_PIN = 17;
        private const int LED_PIN = 18;

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing GPIO");

            using (var gpio = new GpioController())
            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => cts.Cancel();

                gpio.OpenPin(PIR_PIN, PinMode.Input);
                gpio.OpenPin(LED_PIN, PinMode.Output);

                Console.WriteLine("Monitoring PIR sensor. ctrl+c to cancel.");
                bool lastOn = false;

                while (!cts.IsCancellationRequested)
                {
                    bool pirOn = gpio.Read(PIR_PIN) == true;

                    if (lastOn != pirOn)
                    {
                        Console.WriteLine($"Motion sensor is now {(pirOn ? "on" : "off")}");
                        lastOn = pirOn;
                        gpio.Write(LED_PIN, pirOn);
                    }
                }

                Console.WriteLine("Cleaning up");
                gpio.ClosePin(PIR_PIN);
                gpio.ClosePin(LED_PIN);
            }
        }
    }
}