using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateless;

namespace CatSiren
{
    public class MotionSiren : DisposableObject, IMotionSiren
    {
        private readonly SirenSettings settings;
        private readonly ILogger<MotionSiren> logger;
        private readonly IGpioService gpio;
        private readonly ICameraService cameraService;

        private readonly StateMachine<SirenState, SirenTrigger> machine;

        public MotionSiren(
            SirenSettings settings,
            ILogger<MotionSiren> logger,
            IGpioService gpio,
            ICameraService cameraService)
        {
            this.settings = settings;
            this.logger = logger;
            this.gpio = gpio;
            this.cameraService = cameraService;
            this.machine = CreateStateMachine();
        }

        private StateMachine<SirenState, SirenTrigger> CreateStateMachine()
        {
            var machine = new StateMachine<SirenState, SirenTrigger>(
                () => this.State, newState => this.State = newState);

            machine.OnTransitioned(t =>
                this.logger.LogInformation("Siren state has changed from {source} to {dest}",
                    t.Source,
                    t.Destination));

            machine.Configure(SirenState.Warmup)
                .Permit(SirenTrigger.Initialized, SirenState.Idle);

            machine.Configure(SirenState.Idle)
                .Permit(SirenTrigger.SensorHot, SirenState.Active);

            machine.Configure(SirenState.Active)
                .Permit(SirenTrigger.CaptureFootage, SirenState.Recording)
                .OnEntryAsync(OnSirenActivated);

            machine.Configure(SirenState.Recording)
                .SubstateOf(SirenState.Active)
                .Permit(SirenTrigger.FootageCaptured, SirenState.Cooldown)
                .OnEntryAsync(OnCaptureFootage);

            machine.Configure(SirenState.Cooldown)
                .SubstateOf(SirenState.Active)
                .Permit(SirenTrigger.CooldownElapsed, SirenState.Idle)
                .OnEntryAsync(OnCooldown);

            //System.Diagnostics.Debug.WriteLine(Stateless.Graph.UmlDotGraph.Format(machine.GetInfo()));

            return machine;
        }

        public SirenState State { get; private set; }

        public async Task MonitorAsync(CancellationToken stoppingToken)
        {
            await InitializeAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await SetSensorState(this.gpio.Read(this.settings.PirPin));
                await Task.Delay(1000);
                this.LogVerboseState();
            }
        }

        private async Task InitializeAsync(CancellationToken cancelToken)
        {
            // Sensor takes ~1 min to initialize. Any events before then are not completely reliable.
            // Assuming here that app is starting shortly after sensor initializes.
            TimeSpan warmupDelay = TimeSpan.FromSeconds(this.settings.WarmupInterval);

            this.logger.LogInformation("Opening PIR pin {pin}", this.settings.PirPin);
            this.gpio.OpenPinInput(this.settings.PirPin);

            this.logger.LogInformation(
                "Waiting for warmup for {seconds} seconds until {readyTime:G}",
                this.settings.WarmupInterval,
                DateTime.Now.Add(warmupDelay));
            await Task.Delay(warmupDelay, cancelToken);

            this.logger.LogInformation("Warmed up, initialized");
            await this.machine.FireAsync(SirenTrigger.Initialized);
        }

        private void LogVerboseState()
        {
            this.logger.LogTrace(
                "{pirPinState}. Siren state: {sirenState}",
                this.SensorState ? "PIR Pin On" : "PIR Pin Off",
                this.State);
        }

        private async Task SetSensorState(bool isOn)
        {
            this.SensorState = isOn;
            SirenTrigger trigger = isOn ? SirenTrigger.SensorHot : SirenTrigger.SensorCold;

            if (this.machine.CanFire(trigger))
            {
                await this.machine.FireAsync(trigger);
            }
        }

        public bool SensorState { get; private set; }

        protected override void DisposeManagedResources()
        {
            this.logger.LogInformation("Disposing camera service");
            this.cameraService?.Dispose();

            this.logger.LogInformation("Disposing GPIO resources");
            this.gpio?.ClosePin(this.settings.PirPin);
            this.gpio?.Dispose();
        }

        private async Task OnSirenActivated()
        {
            this.logger.LogInformation("Siren activated");
            await Task.Delay(500);
            await this.machine.FireAsync(SirenTrigger.CaptureFootage);
        }

        private Task OnCaptureFootage()
        {
            ThreadPool.QueueUserWorkItem(CaptureFootage);
            return Task.CompletedTask;
        }

        private async void CaptureFootage(object state)
        {
            // TODO: implement camera service and record photos / video here up to X configured period
            this.logger.LogInformation("TODO: capture camera footage now");
            await Task.Delay(TimeSpan.FromSeconds(10));

            await this.machine.FireAsync(SirenTrigger.FootageCaptured);
        }

        private Task OnCooldown()
        {
            Observable.Timer(TimeSpan.FromSeconds(settings.ResetMotionAfter))
                .Select(l => Observable.FromAsync(CompleteCooldown))
                .Concat()
                .Subscribe();

            return Task.CompletedTask;
        }

        private async Task CompleteCooldown()
        {
            await this.machine.FireAsync(SirenTrigger.CooldownElapsed);
        }
    }
}