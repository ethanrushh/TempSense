using System.Diagnostics;
using EthanRushbrook.TempSense.Client.Windows;
using System.Net.NetworkInformation;
using LibreHardwareMonitor.Hardware;

namespace EthanRushbrook.TempSense.Client.Sensors;

// https://github.com/LibreHardwareMonitor/LibreHardwareMonitor
public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

public class WindowsSensors : ISensors
{
    private NetworkStatistics _previousNetworkStats = new()
    {
        DownloadBytes = 0,
        UploadBytes = 0,
        DeltaDownloadBytes = 0,
        DeltaUploadBytes = 0,
        Period = TimeSpan.FromSeconds(1),
        Timestamp = DateTime.UtcNow
    };

    // TODO: Move somewhere more appropriate to keep everything in sync
    private readonly HardwareType[] SupportedHardware = [HardwareType.Cpu];
    
    private readonly Computer _computer;
    private readonly Timer? _cpuUpdateTimer;
    public WindowsSensors()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
        };

        _computer.Open();
        _computer.Accept(new UpdateVisitor());

        var cpuHardware = _computer
            .Hardware
            .Where(h => SupportedHardware.Contains(h.HardwareType))
            .ToList();

        cpuHardware.ForEach(x => x.Update());
        
        // Update every (x) seconds. Necessary because the update takes a long time (~250ms for the CPU sensors)
        _cpuUpdateTimer = new Timer(_ =>
        {
            cpuHardware.ForEach(x => x.Update());
        }, null, 0, 2500);
    }
    public void Dispose()
    {
        _cpuUpdateTimer?.Dispose();
        _computer.Close();
    }

    public double GetSensorValueOrDefault(string deviceName, string sensorName, string fieldName)
    {
        var deviceType = deviceName switch
        {
            "CPU" => HardwareType.Cpu,
            _ => throw new ArgumentException("Unsupported device name", nameof(deviceName))
        };

        var hardware = _computer.Hardware.FirstOrDefault(x => x.HardwareType == deviceType);
        
        if (hardware is null)
            throw new Exception($"Hardware {deviceName} not found");

        var sensorType = sensorName switch
        {
            "Temperature" => SensorType.Temperature,
            _ => throw new Exception("Unsupported sensor type")
        };

        var sensor = hardware.Sensors.FirstOrDefault(x => x.SensorType == sensorType && x.Name == fieldName);

        if (sensor is null)
            throw new Exception($"Sensor ${sensorType}:${fieldName} not found");

        return sensor.Value.GetValueOrDefault();
    }
    
    public MemoryStatistics GetMemoryStats()
    {
        var memStatus = new WindowsInterop.MEMORYSTATUSEX();
        
        if (!WindowsInterop.GlobalMemoryStatusEx(memStatus))
            return new MemoryStatistics
            {
                Available = 0,
                Total = 0,
                UsedPercentage = 0
            };
        
        var total = memStatus.ullTotalPhys;
        var available = memStatus.ullAvailPhys;
            
        return new MemoryStatistics
        {
            Available = available / 1024,
            Total = total / 1024,
            UsedPercentage = (double)(total - available) / total * 100.0
        };
    }
    
    public NetworkStatistics GetNetworkStats(string adapter)
    {
        var nics = NetworkInterface
            .GetAllNetworkInterfaces();
        var nic = nics.FirstOrDefault(x => x.Description.Equals(adapter, StringComparison.OrdinalIgnoreCase));

        if (nic == null)
        {
            return new NetworkStatistics
            {
                DownloadBytes = 0,
                UploadBytes = 0,
                DeltaDownloadBytes = 0,
                DeltaUploadBytes = 0,
                Period = TimeSpan.FromSeconds(1),
                Timestamp = DateTime.UtcNow
            };
        }

        var stats = nic.GetIPv4Statistics();
        var downloadBytes = stats.BytesReceived;
        var uploadBytes = stats.BytesSent;

        var now = DateTime.UtcNow;

        var currentNetworkStats = new NetworkStatistics
        {
            UploadBytes = uploadBytes,
            DownloadBytes = downloadBytes,

            DeltaUploadBytes = uploadBytes - _previousNetworkStats.UploadBytes,
            DeltaDownloadBytes = downloadBytes - _previousNetworkStats.DownloadBytes,

            Timestamp = now,
            Period = now - _previousNetworkStats.Timestamp
        };

        _previousNetworkStats = new NetworkStatistics
        {
            UploadBytes = uploadBytes,
            DownloadBytes = downloadBytes,
            Timestamp = now
        };

        return currentNetworkStats;
    }

    public DiskStatistics GetDiskStats(string directory)
    {
        var root = Path.GetPathRoot(directory);
        
        if (root is null)
            return new DiskStatistics
            {
                Available = 0,
                Total = 0,
                UsedPercentage = 0,
                Used = 0
            };
        
        var drive = new DriveInfo(root);

        if (!drive.IsReady)
            return new DiskStatistics
            {
                Available = 0,
                Total = 0,
                UsedPercentage = 0,
                Used = 0
            };
        
        var freePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;

        return new DiskStatistics
        {
            Available = drive.AvailableFreeSpace,
            Total = drive.TotalSize,
            Used = drive.TotalSize - drive.AvailableFreeSpace,
            UsedPercentage = freePercent
        };
    }
}
