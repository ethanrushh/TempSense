namespace EthanRushbrook.TempSense.Client.Sensors;

public static class SystemSensors
{
    public static ISensors GetAutoSensors(RuntimePlatform platform)
        => platform switch
        {
            #if WINDOWS
            RuntimePlatform.Windows => new WindowsSensors(),
            #endif
            RuntimePlatform.Linux => new LinuxSensors(),
            
            _ => throw new UnsupportedPlatformException()
        };
}
