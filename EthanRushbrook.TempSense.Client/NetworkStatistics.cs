namespace EthanRushbrook.TempSense.Client;

public struct NetworkStatistics
{
    public long UploadBytes;
    public long DownloadBytes;

    public long DeltaUploadBytes;
    public long DeltaDownloadBytes;
    
    public TimeSpan Period;

    public DateTime Timestamp;
    
    public double UploadSpeed => Period.TotalSeconds > 0 ? DeltaUploadBytes / Period.TotalSeconds : 0;
    public double DownloadSpeed => Period.TotalSeconds > 0 ? DeltaDownloadBytes / Period.TotalSeconds : 0;
}
