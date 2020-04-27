namespace CatSiren
{
    public enum SirenState
    {
        Warmup,
        Idle,
        Active,
        Recording,
        Cooldown
    }

    public enum SirenTrigger
    {
        Initialized,
        SensorHot,
        SensorCold,
        CooldownElapsed,
        CaptureFootage,
        FootageCaptured
    }
}