# WSL Hosts Updater
**Windows service that automatically manages `hosts` entries for your Windows Subsystem for Linux (WSL) instance.**

## Features
- Lightweight Windows service
- Periodically checks your default WSL instance's status and IP
- Automatically updates `hosts` entries with the instance's IP address
- Keeps your hosts file clean and only ever adds one line

## Installation

✅ [**Download the latest build from GitHub**](https://github.com/roydejong/WSLHostsUpdater/releases/latest) (or build from source if you prefer)

Place the files in a permanent installation directory.

Install `WSLHostsUpdater.exe` as a Windows Service, and configure it to run under your user account credentials.

## Configuration
You can modify `appsettings.json` to configure some aspects of the service:

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
