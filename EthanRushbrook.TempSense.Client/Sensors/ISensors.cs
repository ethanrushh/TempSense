namespace EthanRushbrook.TempSense.Client.Sensors;

public interface ISensors : IDisposable
{
    public double GetSensorValueOrDefault(string deviceName, string sensorName, string fieldName);
    public MemoryStatistics GetMemoryStats();
    public NetworkStatistics GetNetworkStats(string adapter);
    public DiskStatistics GetDiskStats(string directory);
}
