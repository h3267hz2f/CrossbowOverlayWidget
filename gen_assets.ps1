Add-Type -AssemblyName System.Drawing

function New-PlaceholderPng {
    param([string]$Path, [int]$W, [int]$H)
    $bmp = New-Object System.Drawing.Bitmap($W, $H)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
    $g.Dispose()
    $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Created: $Path ($W x $H)"
}

$base = "c:\Users\Jbao\Desktop\test\CrossbowOverlayWidget\CrossbowOverlayWidget"

New-PlaceholderPng "$base\Assets\LockScreenLogo.scale-200.png" 48 48
New-PlaceholderPng "$base\Assets\SplashScreen.scale-200.png" 1240 600
New-PlaceholderPng "$base\Assets\Square150x150Logo.scale-200.png" 300 300
New-PlaceholderPng "$base\Assets\Square44x44Logo.scale-200.png" 88 88
New-PlaceholderPng "$base\Assets\StoreLogo.png" 50 50
New-PlaceholderPng "$base\Assets\Wide310x150Logo.scale-200.png" 620 300
New-PlaceholderPng "$base\GameBar\icon.png" 48 48

Write-Host "All placeholder images created."
