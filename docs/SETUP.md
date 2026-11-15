# Local setup: Acer + Lenovo + Gaming desktop

## 1. Gaming desktop

- Use wired Ethernet. Enable WoL/magic-packet wake in BIOS and NIC driver settings.
- Confirm Sunshine starts automatically and Tailscale reconnects when the PC wakes.
- Check which host TCP port responds after Sunshine starts (the default Sunshine HTTPS port is often 47984); do not assume a port without testing.
- Enable Windows OpenSSH server if using VS Code Remote SSH, and add an SSH alias named `gaming-desktop` on the Acer.

## 2. Lenovo wake relay (Windows 10)

- Lenovo and desktop must share a broadcast-capable LAN.
- Save `relay/Wake-Desktop.ps1` as `C:\RemoteCompute\Wake-Desktop.ps1`. Edit the desktop's wired Ethernet MAC and subnet broadcast address.
- Sleep the desktop and execute the script **locally on Lenovo** first.
- Enable OpenSSH Server in Windows Optional Features. Start the `sshd` service and configure automatic startup.
- Create a dedicated **non-admin** Windows user with only permissions needed to execute the WoL script.
- Configure SSH public-key login from Acer and restrict the key to a forced command. See `authorized_keys.example`. A forced command overrides the remote command passed by the client.
- Restrict SSH access using Windows Firewall and Tailscale ACLs/grants. The app does not configure firewall rules automatically.

## 3. Acer client

- Generate an SSH key if needed: `ssh-keygen -t ed25519`. Enroll only its **public** key on Lenovo.
- Verify `ssh -o BatchMode=yes YOUR_LENOVO_USER@LENOVO_TAILSCALE_IP wake-desktop` prints `Wake packet sent` without a password prompt.
- Verify desktop readiness: `Test-NetConnection DESKTOP_TAILSCALE_IP -Port 47984` (use the actual configured port).
- Install .NET 10 SDK on Windows and build `src/RemoteCompute.Client`.
- Update `src/RemoteCompute.Client/appsettings.json` with your Tailscale IPs, usernames, Moonlight/VS Code paths, SSH host alias and readiness port.
- Start the client and select **Play Games** or **Start Coding**.

## Security and limitations

- Do **not** port-forward SSH publicly. Use Tailscale/private networking and authenticate with SSH keys.
- Confirm SSH host keys out-of-band on first connection to prevent spoofing.
- Never commit real user addresses, private keys, access tokens or secrets to this repository.
- This alpha only wakes and launches. It does not track sessions or put the desktop to sleep.
- The host must already have Sunshine or SSH configured independently.

## Troubleshooting

- SSH failed: check Tailscale, sshd, firewall, key/forced-command and username.
- WoL sent but no wake: check wired MAC, subnet broadcast, BIOS, NIC sleep support and Windows power configuration.
- Woke but timed out: verify desktop Tailscale connectivity, selected readiness TCP port and firewall.
- Moonlight opened but no stream: complete Moonlight pairing with Sunshine first.
- VS Code cannot connect: verify OpenSSH server on desktop and local SSH alias.
