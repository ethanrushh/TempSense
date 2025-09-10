namespace EthanRushbrook.TempSense.Client;

public struct MemoryStatistics
{
    /// <summary>
    /// Total memory, in KiB
    /// </summary>
    public ulong Total;
    /// <summary>
    /// Available memory, in KiB
    /// </summary>
    public ulong Available;
    /// <summary>
    /// Memory usage [0, 100]
    /// </summary>
    public double UsedPercentage;
}