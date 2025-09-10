using System.Diagnostics;
using System.Text.Json;

namespace EthanRushbrook.TempSense.Client.Sensors;

public class LinuxSensors : ISensors
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
    
    public void Dispose() { }

    public double GetSensorValueOrDefault(string deviceName, string sensorName, string fieldName)
    {
        var sensorRun = JsonDocument.Parse(RunSensors());
        
        if (sensorRun.RootElement.TryGetProperty(deviceName, out JsonElement coretemp))
        {
            if (coretemp.TryGetProperty(sensorName, out JsonElement package))
            {
                if (package.TryGetProperty(fieldName, out JsonElement temp))
                {
                    return temp.GetDouble();
                }
            }
        }

        return 0;
    }

    public void ListSensors()
        => throw new NotImplementedException();

    private static string RunSensors()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "sensors",
            Arguments = "-j",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);

        return process is null ? throw new Exception("Unable to find sensors") : process.StandardOutput.ReadToEnd();
    }
    
    public MemoryStatistics GetMemoryStats()
    {
        ulong memTotal = 0;
        ulong memAvailable = 0;

        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (line.StartsWith("MemTotal:"))
                memTotal = ulong.Parse(line.Split([' '], StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("MemAvailable:"))
            {
                memAvailable = ulong.Parse(line.Split([' '], StringSplitOptions.RemoveEmptyEntries)[1]);
                break;
            }
        }

        if (memTotal == 0)
            throw new Exception("Unable to detect total memory");

        return new MemoryStatistics
        {
            Available = memAvailable,
            Total = memTotal,
            UsedPercentage = (double)(memTotal - memAvailable) / memTotal * 100.0
        };
    }
    
    public NetworkStatistics GetNetworkStats(string adapter)
    {
        var adapterStats = File.ReadLines("/proc/net/dev").FirstOrDefault(x => x.Contains(adapter + ":"));

        if (adapterStats is null)
            return new NetworkStatistics
            {
                DownloadBytes = 0,
                UploadBytes = 0,
                DeltaDownloadBytes = 0,
                DeltaUploadBytes = 0,
                Period = TimeSpan.FromSeconds(1),
                Timestamp = DateTime.UtcNow
            };
        
        var parts = adapterStats.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);
        var downloadBytes = long.Parse(parts[1]);
        var uploadBytes = long.Parse(parts[9]);
        
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
