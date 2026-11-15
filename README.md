# Remote Compute

Free, open-source Windows native remote PC launcher. Wake a desktop from your laptop through an always-on Wake-on-LAN relay, then open Moonlight or VS Code Remote SSH.

> **v0.1 alpha** — starter implementation, not production ready or hardware tested. Wake and launch only; automatic sleep and a host agent are planned.

## Components

- `src/RemoteCompute.Client`: .NET 10 / WPF launcher
- `relay/Wake-Desktop.ps1`: Windows PowerShell 5.1 compatible WoL relay
- `docs/SETUP.md`: setup and security instructions

## Build

On Windows with the .NET 10 SDK:

```powershell
dotnet build .\src\RemoteCompute.Client\RemoteCompute.Client.csproj
dotnet run --project .\src\RemoteCompute.Client\RemoteCompute.Client.csproj
```

Edit `appsettings.json` with your own device addresses, SSH username, paths and verified Sunshine readiness port before running. This first release requires SSH key authentication and existing Tailscale/private connectivity. **Never expose the relay's SSH port directly to the public internet.**

## Roadmap

1. Validate WoL and readiness on real hardware.
2. Add host agent, authenticated session leases, and sleep safeguards.
3. Add tray integration, diagnostics, setup wizard, packaging and automated hardware tests.

MIT licensed. Contributions welcome.
