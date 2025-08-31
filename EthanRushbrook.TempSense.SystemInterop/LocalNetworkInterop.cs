using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace EthanRushbrook.TempSense.SystemInterop;

public class LocalNetworkInterop
{
    public static string? GetLocalIPv4()
    {
        return (from ni in NetworkInterface.GetAllNetworkInterfaces()
            where ni.OperationalStatus == OperationalStatus.Up &&
                  ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
            select ni.GetIPProperties()
            into ipProps
            from addr in ipProps.UnicastAddresses
            where addr.Address.AddressFamily == AddressFamily.InterNetwork
            select addr.Address.ToString()).FirstOrDefault();
    }
}
