<#
.SYNOPSIS
    Builds and runs the Melissa Reverse GeoCoder Cloud API .NET sample.

.DESCRIPTION
    This script builds ReverseGeoCoderDotnet with dotnet publish, then runs the
    resulting executable, passing along the license and (if supplied) the latitude,
    longitude, and max records.

    Overall flow:
      1. Resolve the license (parameter, prompt, or MD_LICENSE environment variable).
      2. Publish ReverseGeoCoderDotnet in Release configuration to
         .\ReverseGeoCoderDotnet\Build.
      3. Run the built executable: one-shot mode if any of -lat, -long, or -max was supplied,
         otherwise interactive mode (the .NET program prompts for each field).

.PARAMETER lat
    Latitude to look up in one-shot mode.

.PARAMETER long
    Longitude to look up in one-shot mode.

.PARAMETER max
    Maximum number of records to return in one-shot mode.

.PARAMETER license
    License string. Resolved in this order:
      1. This parameter.
      2. An interactive prompt, if the parameter was not supplied.
      3. The MD_LICENSE environment variable, if the prompt was left blank.
    Note that the environment variable is the last resort, not the first: running
    without -license always prompts, even when MD_LICENSE is set.

.PARAMETER quiet
    Accepted for parity with other sample scripts; not currently used to suppress output.

.EXAMPLE
    .\ReverseGeoCoderDotnet.ps1 -license "your-license"

.EXAMPLE
    .\ReverseGeoCoderDotnet.ps1 -lat "33.637520" -long "-117.606920" -max "3" -license "your-license"
#>

######################### Parameters ##########################
param(
    $lat = '',
    $long = '',
    $max = '',
    $license = '',
    [switch]$quiet = $false
    )

# Uses the location of the .ps1 file
$CurrentPath = $PSScriptRoot
Set-Location $CurrentPath
$ProjectPath = "$CurrentPath\ReverseGeoCoderDotnet"
$BuildPath = "$ProjectPath\Build"

If (!(Test-Path $BuildPath)) {
  New-Item -Path $ProjectPath -Name 'Build' -ItemType "directory"
}

########################## Main ############################
Write-Host "`n====================== Melissa Reverse GeoCoder Cloud API ======================`n"

# Get license (either from parameters or user input)
if ([string]::IsNullOrEmpty($license) ) {
  $license = Read-Host "Please enter your license string"
}

# Check for License from Environment Variables 
if ([string]::IsNullOrEmpty($license) ) {
  $license = $env:MD_LICENSE 
}

if ([string]::IsNullOrEmpty($license)) {
  Write-Host "`nLicense String is invalid!"
  Exit
}

# Start program
# Build project
Write-Host "`n================================= BUILD PROJECT ================================"

dotnet publish -f="net7.0" -c Release -o $BuildPath ReverseGeoCoderDotnet\ReverseGeoCoderDotnet.csproj

# Run project
# No lookup fields supplied -> run interactively; otherwise pass the supplied ones through for one-shot mode.
if ([string]::IsNullOrEmpty($lat) -and [string]::IsNullOrEmpty($long) -and [string]::IsNullOrEmpty($max)) {
  dotnet $BuildPath\ReverseGeoCoderDotnet.dll --license $license
}
else {
  # Only pass flags that have a value. Windows PowerShell drops empty-string arguments to
  # native programs, which would shift the next flag name into this flag's value.
  # Any field left out here is prompted for by the program.
  $runArgs = @('--license', $license)
  if (-not [string]::IsNullOrEmpty($lat))  { $runArgs += '--lat', $lat }
  if (-not [string]::IsNullOrEmpty($long)) { $runArgs += '--long', $long }
  if (-not [string]::IsNullOrEmpty($max))  { $runArgs += '--max', $max }
  dotnet $BuildPath\ReverseGeoCoderDotnet.dll @runArgs
}
