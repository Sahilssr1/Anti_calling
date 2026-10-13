# SecureNXG AUTOMATIC INSTALLER — runs ON THE CUSTOMER'S (remote) PC.
#
# Started by the agent's ONE click ("Auto Install SecureNXG") on the calling
# page. The agent PC types a short bootstrap into the remote PC's PowerShell,
# which downloads THIS file from GitHub and runs it.
#
# What it does:
#   1. Downloads the installer from the SecureNXG download URL.
#      (Handles both cases: direct file download, or a web page with an
#      .exe/.msi link on it.)
#   2. Detects the installer type (MSI vs EXE, NSIS vs Inno Setup) and runs it
#      SILENTLY — no wizard clicks needed.
#   3. Verifies the install (service / process / uninstall registry / folder)
#      and prints a BIG result on screen (the agent sees it via AnyDesk).
#
# The ONE thing that stays manual: if Windows shows a UAC prompt ("Do you want
# to allow this app to make changes?"), the customer must click YES once.
# Remote tools cannot click UAC — that is a Windows security rule.
# Log: $env:TEMP\snxg\install.log

$ErrorActionPreference = 'Stop'
$DownloadUrl = 'https://securex.we6.in/Download/SecureNXG_Setup'
$TempDir = Join-Path $env:TEMP 'snxg'
$LogFile = Join-Path $TempDir 'install.log'
$null = New-Item -ItemType Directory -Force -Path $TempDir

function Log($msg) {
    $line = '[{0}] {1}' -f (Get-Date -Format 'HH:mm:ss'), $msg
    Write-Host $line
    Add-Content -Path $LogFile -Value $line
}

function Show-Result($ok, $msg) {
    Write-Host ''
    Write-Host '==============================' -ForegroundColor $(if ($ok) { 'Green' } else { 'Red' })
    Write-Host "  RESULT: $(if ($ok) { 'INSTALLED' } else { 'FAILED' }) - $msg" -ForegroundColor $(if ($ok) { 'Green' } else { 'Red' })
    Write-Host '==============================' -ForegroundColor $(if ($ok) { 'Green' } else { 'Red' })
    Write-Host "  Log file: $LogFile"
}

function Test-Installed {
    # Any one of these proves SecureNXG is on the machine.
    if (Get-Service -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'Secure|NxG' }) { return $true }
    if (Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match 'Secure|NxG' }) { return $true }
    $un = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
          Where-Object { $_.DisplayName -match 'Secure' }
    if ($un) { return $true }
    if (Test-Path 'C:\Program Files\Secure*') { return $true }
    if (Test-Path 'C:\Program Files (x86)\Secure*') { return $true }
    return $false
}

function Resolve-Link($base, $link) {
    if ($link -match '^https?://') { return $link }
    $b = [System.Uri]$base
    if ($link.StartsWith('/')) { return "$($b.Scheme)://$($b.Host)$link" }
    $dir = $b.AbsoluteUri.Substring(0, $b.AbsoluteUri.LastIndexOf('/') + 1)
    return $dir + $link
}

try {
    Log 'SecureNXG auto-install shuru...'
    Log "Download URL: $DownloadUrl"
    Write-Host 'NOTE: UAC permission aaye to YES dabao (ek baar).' -ForegroundColor Yellow

    if (Test-Installed) {
        Log 'SecureNXG pehle se installed hai.'
        Show-Result $true 'pehle se installed tha'
        exit 0
    }

    # ---- 1. DOWNLOAD ----
    # Download straight to a file first (no memory bloat on big installers),
    # then check whether what we got is an installer or an HTML page.
    Log 'Installer download ho raha hai...'
    $installer = Join-Path $TempDir 'SecureNXG_Setup.bin'
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $installer -UseBasicParsing -TimeoutSec 300

    $probe = [System.IO.File]::ReadAllBytes($installer)
    $probeText = [System.Text.Encoding]::ASCII.GetString($probe, 0, [Math]::Min(1024, $probe.Length))
    if ($probeText -match '<html|<!DOCTYPE') {
        # It's a web page — find the real installer link on it.
        Log 'Download page mili, installer link dhoondh raha hun...'
        $html = [System.IO.File]::ReadAllText($installer)
        $links = @()
        foreach ($pat in 'href\s*=\s*"([^"]+\.(exe|msi))"', "href\s*=\s*'([^']+\.(exe|msi))'") {
            foreach ($m in [regex]::Matches($html, $pat, 'IgnoreCase')) { $links += $m.Groups[1].Value }
        }
        if (-not $links) { throw 'Download page par .exe/.msi link nahi mila — manual download karo.' }
        $fileUrl = Resolve-Link $DownloadUrl $links[0]
        Log "Installer link: $fileUrl"
        Invoke-WebRequest -Uri $fileUrl -OutFile $installer -UseBasicParsing -TimeoutSec 300
    }
    Log "Download ho gaya: $installer"

    # ---- 2. DETECT INSTALLER TYPE ----
    $fs = [System.IO.File]::OpenRead($installer)
    $magic = New-Object byte[] 4
    $null = $fs.Read($magic, 0, 4)
    $fs.Close()
    $isMsi = ($magic[0] -eq 0xD0 -and $magic[1] -eq 0xCF -and $magic[2] -eq 0x11 -and $magic[3] -eq 0xE0)

    if ($isMsi) {
        $kind = 'MSI'
        $args = "/i `"$installer`" /quiet /norestart"
        $exe = 'msiexec.exe'
    }
    else {
        # EXE: read first 4 MB and look for the installer framework marker.
        $fs2 = [System.IO.File]::OpenRead($installer)
        $buf = New-Object byte[] 4MB
        $read = $fs2.Read($buf, 0, $buf.Length)
        $fs2.Close()
        $head = [System.Text.Encoding]::ASCII.GetString($buf, 0, $read)
        if ($head -match 'Nullsoft') {
            $kind = 'NSIS'
            $args = '/S'
        }
        elseif ($head -match 'Inno Setup') {
            $kind = 'Inno Setup'
            $args = '/VERYSILENT /NORESTART /SUPPRESSMSGBOXES'
        }
        else {
            $kind = 'EXE (unknown framework)'
            $args = '/S'
        }
        $exe = $installer
    }
    Log "Installer type: $kind — silent flags: $args"

    # ---- 3. SILENT INSTALL (elevated; UAC = customer's one click) ----
    Log 'Silent install chal raha hai... (UAC aaye to YES dabao)'
    try {
        $p = Start-Process -FilePath $exe -ArgumentList $args -Verb RunAs -PassThru
    }
    catch {
        throw "Installer start nahi hua (UAC me No daba diya gaya ho sakta hai): $($_.Exception.Message)"
    }
    if (-not $p.WaitForExit(300000)) {
        try { $p.Kill() } catch {}
        throw 'Installer 5 minute me finish nahi hua.'
    }
    Log "Installer exit code: $($p.ExitCode)"

    # ---- 4. VERIFY ----
    Log 'Install verify ho raha hai...'
    $installed = $false
    for ($i = 0; $i -lt 6; $i++) {
        if (Test-Installed) { $installed = $true; break }
        Start-Sleep -Seconds 10
    }
    if ($installed) {
        Log 'Verification OK — SecureNXG installed hai.'
        Show-Result $true 'install + verification OK'
        exit 0
    }
    else {
        throw 'Installer chala, lekin SecureNXG ka service/process/registry nahi mila.'
    }
}
catch {
    Log "ERROR: $($_.Exception.Message)"
    Show-Result $false $_.Exception.Message
    exit 1
}
