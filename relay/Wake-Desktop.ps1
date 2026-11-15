# Windows PowerShell 5.1 compatible Wake-on-LAN relay.
# Edit MAC and broadcast address for your LAN.
$ErrorActionPreference = 'Stop'
$MacAddress = 'AA-BB-CC-DD-EE-FF'
$BroadcastAddress = '192.168.0.255'
$hex = $MacAddress -replace '[:-]', ''
if ($hex -notmatch '^[0-9A-Fa-f]{12}$') { throw 'Invalid desktop MAC address' }
$mac = [byte[]]@(0..5 | ForEach-Object { [Convert]::ToByte($hex.Substring($_ * 2, 2), 16) })
[byte[]]$packet = @([byte]255) * 6 + ($mac * 16)
$udp = New-Object System.Net.Sockets.UdpClient
try {
    $udp.EnableBroadcast = $true
    $target = New-Object System.Net.IPEndPoint ([System.Net.IPAddress]::Parse($BroadcastAddress)), 9
    [void]$udp.Send($packet, $packet.Length, $target)
    Write-Output 'Wake packet sent'
} finally {
    $udp.Close()
}
