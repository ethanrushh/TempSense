[![TempSense Banner](banner.png)]("https://tempsense-site.vercel.app/")

# TempSense - Local, free, open source monitoring for all your devices

## What is TempSense?

TempSense is an application that allows you to stream various sensor data to a single device, ideally one with a touch display, for viewing. An example use-case: Gaming PC + Streaming PC + 2x Homelab Proxmox Instances -> Raspberry Pi 5B with Touch Display. 

TempSense supports most Linux distributions and Windows. Stream realtime sensor data from multiple devices to a single instance. 

## How do I use TempSense?

Proper documentation is on the way. Hold tight. For now, feel free to poke at `ConfigModel.cs` and refer to the development `config.json` files for an idea on how to set things up. You can call the client with `--list-sensors` to see what is available on your system.

## Dependencies
You will need `lm-sensors` for Linux. Windows should be entirely self-contained and dependency free. 

## FAQ

### Supported Platforms
Windows, Linux (Ubuntu and Arch tested)

### Is TempSense Free?

Yes. TempSense is completely free (see disclaimer)

### Am I allowed to contribute?

Contributions are welcome! Open your first Issue or Pull Request and I'll take a look ASAP.

### Whats planned for this project?

This project started as and continues to be purely a passion project out of personal necessity. Actions (referenced on the site) are coming but are not here yet. That will allow TempSense to be used for more than the name would suggest. That would turn it into competition for a certain popular stream aid.

### How do I use "Actions" like I saw on the site

Actions is still in development. Check back soon for progress.

## Building

Recommended platform IDs are linux-x64 for Linux and win-x64 for Windows.

```
git clone https://github.com/ethanrushh/TempSense
cd TempSense
dotnet restore
```
#### For Server/UI:
```
cd EthanRushbrook.TempSense.Server
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true -r <platform id>
```

#### For Client:
```
cd EthanRushbrook.TempSense.Client
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true -r <platform id>
```

.NET will tell you in the console output where to find the resulting binaries. 

## Disclaimer
This is prototype, personal project software; I am not responsible for any damages that may occur from the use of this application. Use at your own risk. Refer to the license for more information.
