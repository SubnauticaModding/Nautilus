[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]$SolutionDir,
    [Parameter(Mandatory)]
    [string]$ConfigurationName,
    [Parameter(Mandatory)]
    [string]$TargetDir,
    [Parameter(Mandatory)]
    [string]$ProjectDir
)

<#
    Creates or updates a zip archive

    Takes three arguments:
    -Path is the string path of the the files/folder to add the zip archive
    -DestinationPath is the string path to the output .zip archive
    -Fresh is a toggle. If set, the archive will be deleted prior to archiving. Otherwise, the new contents will be added to the archive as-is.
#>
function Zip
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)]
        [string]$Path,
        [Parameter(Mandatory)]
        [string]$DestinationPath,
        [switch]$Fresh
    )

    if ($Fresh -and (Test-Path $DestinationPath)) {
        $null = Remove-Item -Path $DestinationPath
    }
    
    $7ZipPath = [System.IO.Path]::Combine($SolutionDir, "packages", "7-Zip.CommandLine.18.1.0", "tools", "7za.exe")
    if (Test-Path $7ZipPath)
    {   # Use 7Zip if available
        & $7ZipPath "u" $DestinationPath $Path
    }
    else
    {   # Otherwise fallback to Compress-Archive
        if ($Fresh)
        {
            Compress-Archive -Path $Path -DestinationPath $DestinationPath -Force
        }
        else
        {
            Compress-Archive -Path $Path -DestinationPath $DestinationPath -Update
        }
    }
}

$buildPath = switch ($ConfigurationName.ToUpper())
{
    {($_ -like "SN*")} { "Modding Helper" }
    {($_ -like "BZ*")} { "SMLHelper_BZ" }
    default { "Modding Helper" }
}

$pluginsDir = [System.IO.Path]::Combine($ProjectDir, "bin", $ConfigurationName, "plugins")

# Remove prep dir and create fresh
if (Test-Path $pluginsDir)
{
    $null = Remove-Item -Path $pluginsDir -Force -Recurse
}
$null = New-Item -Path $pluginsDir -ItemType "directory"

$buildDir = [System.IO.Path]::Combine($pluginsDir, $buildPath)
$null = New-Item -Path $buildDir -ItemType "directory"

# Copy core mod files to build dir
foreach ($file in "SMLHelper.xml", "SMLHelper.dll")
{
    Copy-Item $([System.IO.Path]::Combine($TargetDir, $file)) -Destination $buildDir
}

# Zip the standard QMod build
$buildZipPath = [System.IO.Path]::Combine($TargetDir, "SMLHelper_$($ConfigurationName).zip")
$null = Zip -Path $pluginsDir -DestinationPath $buildZipPath -Fresh

# Zip the Thunderstore build
$thunderstoreMetadataPath = [System.IO.Path]::Combine($ProjectDir, "ThunderstoreMetadata", $ConfigurationName, "*")
$thunderstoreZipPath = [System.IO.Path]::Combine($TargetDir, "SMLHelper_$($ConfigurationName)_Thunderstore.zip")
$null = Zip -Path $pluginsDir -DestinationPath $thunderstoreZipPath -Fresh
$null = Zip -Path $thunderstoreMetadataPath -DestinationPath $thunderstoreZipPath