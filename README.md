# WSL Hosts Updater
**Windows service that automatically manages `hosts` entries for your Windows Subsystem for Linux (WSL) instance.**

## Features
- Lightweight Windows service
- Periodically checks your default WSL instance's status and IP
- Automatically updates `hosts` entries with the instance's IP address
- Keeps your hosts file clean and only ever adds one line per host

## Installation

✅ [**Download the latest build from GitHub**](https://github.com/roydejong/WSLHostsUpdater/releases/latest) (or build from source if you prefer)

You can either run the program manually, or install it as a Windows Service. Either way, you'll need administrator permissions so the `hosts` file can be edited.

### Service installation
Place the files in a permanent directory, then use `sc` on the command line to install the Windows service:

```
sc.exe create "WSLHostsUpdater" binpath="<SET_YOUR_INSTALL_PATH>\WSLHostsUpdater.exe" start=delayed-auto obj=<SET_YOUR_WINDOWS_ACCOUNT> password=<SET_YOUR_WINDOWS_PASSWORD>
```

After this, you'll be able to see and manage the service from the Windows Services view. Warnings and errors will appear in the event viewer with `WSLHostsUpdater` as source.

## Configuration
You can modify `appsettings.json` (must be in the same directory as the executable) to configure some aspects of the service:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Service": {
    "CheckWslRunning": true,
    "UpdateInterval": 30000,
    "Hostnames": ["host1.com", "host2.com"]
  }
}
```

| Key               | Description                                                          | Default                       |
|-------------------|----------------------------------------------------------------------|-------------------------------|
| `CheckWslRunning` | If true, check if WSL is running first before attempting to refresh. | `true`                        |
| `UpdateInterval`  | The amount of time in milliseconds between refreshes.                | `30000` (30 seconds)          |
| `Hostnames`       | Array of host names to update in the `hosts` file.                   | `["wsl.local", "ubuntu.wsl"]` |

🕑 The configuration file is hot reloaded automatically, so you can add hostnames whenever you like without having to restart the service.