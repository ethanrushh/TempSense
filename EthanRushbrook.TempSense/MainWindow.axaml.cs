using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EthanRushbrook.TempSense.SystemInterop;
using SukiUI.Controls;

namespace EthanRushbrook.TempSense;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        InitializeWindowState();
        DetectLocalIp();
    }

    private void InitializeWindowState()
    {
        if (!File.Exists("/proc/cpuinfo"))
            return;

        var lines = File.ReadAllLines("/proc/cpuinfo");

        // Look for Hardware, Model, or Revision fields
        var hardware = lines.FirstOrDefault(l => l.StartsWith("Hardware"));
        var model = lines.FirstOrDefault(l => l.StartsWith("Model"));

        // Raspberry Pi 5 has BCM2712 SoC
        if ((hardware?.Contains("BCM2712") ?? false) ||
            (model?.Contains("Raspberry Pi 5") ?? false))
        {
            Console.WriteLine("CPU is a BCM2712: Setting to fullscreen (Pi 5 detected)");
            
            WindowState = WindowState.FullScreen;
        }
        else
            Console.WriteLine("Pi 5 not detected, starting in dev mode");
    }

    private void DetectLocalIp()
    {
        var localIp = LocalNetworkInterop.GetLocalIPv4();
        
        LocalIpHeader.Header = localIp ?? "No IPv4 detected";
    }
}
