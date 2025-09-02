using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json;

namespace EthanRushbrook.TempSense.Client.Linux;

public class LinuxSensors : IDisposable
{
    private readonly string _networkAdapterName;
    private JsonDocument? _lastSensorRun;
    private MemoryStatistics? _lastMemoryStats;
    private NetworkStatistics? _currentNetworkStats;
    private NetworkStatistics _previousNetworkStats;

    public LinuxSensors(string networkAdapterName)
    {
        _networkAdapterName = networkAdapterName;
        var (uploadBytes, downloadBytes) = GetNetworkBytes(networkAdapterName);
        
        _previousNetworkStats = new NetworkStatistics
        {
            DownloadBytes = 0,
            UploadBytes = uploadBytes,
            DeltaDownloadBytes = downloadBytes,
            DeltaUploadBytes = 0,
            Period = TimeSpan.FromSeconds(1),
            Timestamp = DateTime.UtcNow
        };
    }
    
    public void Tick()
    {
        _lastMemoryStats = UpdateMemoryStats();
        _lastSensorRun = JsonDocument.Parse(RunSensors());
        
        var (uploadBytes, downloadBytes) = GetNetworkBytes(_networkAdapterName);
        var now = DateTime.UtcNow;

        _currentNetworkStats = new NetworkStatistics
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
    }

    public double? GetSensorValueOrDefault(string deviceName, string sensorName, string fieldName)
    {
        if (_lastSensorRun is null)
            throw new Exception("Run Tick before reading sensor");
        
        if (_lastSensorRun.RootElement.TryGetProperty(deviceName, out JsonElement coretemp))
        {
            if (coretemp.TryGetProperty(sensorName, out JsonElement package))
            {
                if (package.TryGetProperty(fieldName, out JsonElement temp))
                {
                    return temp.GetDouble();
                }
            }
        }

        return null;
    }

    [Pure]
    public MemoryStatistics GetMemoryStats() => _lastMemoryStats ?? throw new Exception("Run Tick before reading memory");
    [Pure]
    public NetworkStatistics GetNetworkStats() => _currentNetworkStats ?? throw new Exception("Run Tick before reading network");

    public void Dispose() => _lastSensorRun?.Dispose();

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
    
    private static MemoryStatistics UpdateMemoryStats()
    {
        long memTotal = 0;
        long memAvailable = 0;

        foreach (var line in File.ReadLines("/proc/meminfo"))
        {
            if (line.StartsWith("MemTotal:"))
                memTotal = long.Parse(line.Split([' '], StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("MemAvailable:"))
            {
                memAvailable = long.Parse(line.Split([' '], StringSplitOptions.RemoveEmptyEntries)[1]);
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
    
    private static (long UploadBytes, long DownloadBytes) GetNetworkBytes(string adapter)
    {
        foreach (var line in File.ReadLines("/proc/net/dev"))
        {
            if (!line.Contains(adapter + ":")) 
                continue;
            
            var parts = line.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries);
            var downloadBytes = long.Parse(parts[1]);
            var uploadBytes = long.Parse(parts[9]);

            return (uploadBytes, downloadBytes);
        }

        throw new Exception("Network adapter not found or readable");
    }

    public DiskStatistics? GetDiskStats(string directory)
    {
        var root = Path.GetPathRoot(directory);
        
        if (root is null)
            throw new ArgumentException("Unable to find root directory",  nameof(directory));
        
        var drive = new DriveInfo(root);

        if (!drive.IsReady)
            return null;
        
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
