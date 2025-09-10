using System.Runtime.InteropServices;

namespace EthanRushbrook.TempSense.Client;

public class PlatformDetection
{
    public static RuntimePlatform DetectOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return RuntimePlatform.Linux;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return RuntimePlatform.Windows;
        
        throw new UnsupportedPlatformException();
    }
}

public class UnsupportedPlatformException : Exception;

// We need a compile-time constant value for the switch expression which OSPlatform is not.
public enum RuntimePlatform
{
    Windows,
    Linux
}
