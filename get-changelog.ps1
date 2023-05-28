# Copyright (c) 2023 Sander van Vliet
# Licensed under Artistic License 2.0
# See LICENSE or https://choosealicense.com/licenses/artistic-2.0/
param([string]$currentVersion = $(throw "currentVersion is required"))

$gitSha = $env:GITHUB_SHA
$workflow = $env:GITHUB_WORKFLOW

$parts = $currentVersion.Split(".")
$currentMajor = $parts[0]
$currentMinor = $parts[1]
$currentPatch = $parts[2]
$currentBuild = $parts[3]

$lines = get-content Changelog.md

$started = $false
$output = @()

for($index = 0; $index -lt $lines.Length; $index++)
{
    $line = $lines[$index].Trim()
    if($line.StartsWith("## ") -and !$started)
    {
        $parts = $line.Substring(2).Trim().Split(".")
        $major = $parts[0]
        $minor = $parts[1]
        $patch = $parts[2]
        $build = $parts[3]

        if($major -eq $currentMajor -and $minor -eq $currentMinor -and $patch -eq $currentPatch)
        {
            $started = $true
        }
    }
    elseif($started -and $line.StartsWith("## "))
    {
        $parts = $line.Substring(2).Trim().Split(".")
        $major = $parts[0]
        $minor = $parts[1]
        $patch = $parts[2]
        $build = $parts[3]

        if($major -eq $currentMajor -and $minor -eq $currentMinor -and $patch -eq $currentPatch)
        {
            continue
        }
        
        break
    }
    elseif($started) 
    {
        $output += $line
    }
}

$output > version-changelog.md

if($workflow -eq "pre_release_debug")
{
    $(
        "# Pre-release ${currentVersion}+g${gitSha}"
        ""
        "This is a pre-release build. Be aware that there may be rough edges and the occasional bug."
        "Enjoy testing!"
        ""
        (Get-Content version-changelog.md -Raw)
    ) | Set-Content version-changelog.md
}
