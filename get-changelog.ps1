param([string]$currentVersion = $(throw "currentVersion is required"))

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