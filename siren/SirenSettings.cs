namespace CatSiren
{
    public class SirenSettings
    {
        /// <summary>
        /// Gets or sets a value indicating the number of seconds at startup to use as a warmup
        /// period for Infrared sensor before it'll report accurate readings.
        /// </summary>
        /// <value>Number representing time in seconds</value>
        public ushort WarmupInterval { get; set; } = 90;

        /// <summary>
        /// Gets or sets a value indicating the number of seconds after which to start triggering
        /// motion detection again after it was last triggered. Effectively a cool down period.
        /// </summary>
        /// <value>Number representing time in seconds</value>
        public ushort ResetMotionAfter { get; set; } = 15;

        /// <summary>
        /// Gets or sets a value indicating the number of seconds to capture camera media after
        /// motion has been triggered. Allows multiple photos to be taken.
        /// </summary>
        /// <value>Number representing time in seconds</value>
        public ushort CaptureDuration { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value indicating the logical GPIO pin for the motion sensor.
        /// </summary>
        /// <value>Number indicating logical GPIO pin on the PI</value>
        public byte PirPin { get; set; } = 17;

        /// <summary>
        /// Gets or sets a value indicating the path that camera output will be written to.
        /// </summary>
        /// <value>String representing directory path</value>
        public string MediaPath { get; set; } = "/var/lib/siren/media";

        /// <summary>
        /// Gets or sets a value indicating the number of days after which to delete previously
        /// captured siren footage.
        /// </summary>
        /// <value>Double value indicating the number of days</value>
        public double MediaCleanupAfterDays { get; set; } = 1.0;
    }
}