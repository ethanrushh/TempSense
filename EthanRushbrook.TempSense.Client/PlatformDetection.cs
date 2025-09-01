using System.Runtime.InteropServices;

namespace EthanRushbrook.TempSense.Client;

public class PlatformDetection
{
    public static OSPlatform DetectOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OSPlatform.Linux;
        
        throw new UnsupportedPlatformException();
    }
}

public class UnsupportedPlatformException : Exception;
