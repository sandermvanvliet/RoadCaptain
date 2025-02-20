# Copyright (c) 2025 Sander van Vliet
# Licensed under Artistic License 2.0
# See LICENSE or https://choosealicense.com/licenses/artistic-2.0/
$csharpFiles = Get-ChildItem -Recurse -Filter *.cs | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "csharp-license.txt" -Raw
$licenseFirstLine = (Get-Content "csharp-license.txt" -head 1).Trim()

foreach($file in $csharpFiles)
{
    $contents = get-content $file -head 1
    $contentsTrimmed = $contents.Trim()
    
    if(!$contentsTrimmed.StartsWith("// Copyright (c) "))
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header, adding it"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents
        
        $targetContent | Out-File $file -NoNewline
    }
    elseif($contentsTrimmed -ne $licenseFirstLine)
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath has a license header but it's not up to date, changing it"

        $rawContents = Get-Content $file -Raw
        
        $targetContent = $rawContents.Replace($contents.Trim(), $licenseFirstLine)

        $targetContent | Out-File $file -NoNewline
    }
}

$xamlFiles = Get-ChildItem -Recurse -Filter *.axaml | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "xaml-license.txt" -Raw
$licenseFirstLine = (Get-Content "xaml-license.txt" | Select-Object -Skip 1 -First 1).Trim()

foreach($file in $xamlFiles)
{
    $contents = (get-content $file | Select-Object -Skip 1 -First 1)
    $contentsTrimmed = $contents.Trim()
    if(!$contentsTrimmed.StartsWith("// Copyright (c) "))
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents

        $targetContent | Out-File $file -NoNewline
    }
    elseif($contentsTrimmed -ne $licenseFirstLine)
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath has a license header but it's not up to date, changing it"

        $rawContents = Get-Content $file -Raw

        $targetContent = $rawContents.Replace($contentsTrimmed, $licenseFirstLine)

        $targetContent | Out-File $file -NoNewline
    }
}

$powershellFiles = Get-ChildItem -Recurse -Filter *.ps1 | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "powershell-license.txt" -Raw
$licenseFirstLine = (Get-Content "powershell-license.txt" -head 1).Trim()

foreach($file in $powershellFiles)
{
    $contents = get-content $file -head 1
    $contentsTrimmed = $contents.Trim()
    if(!$contentsTrimmed.StartsWith("# Copyright (c) "))
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents

        $targetContent | Out-File $file -NoNewline
    }
    elseif($contentsTrimmed -ne $licenseFirstLine)
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath has a license header but it's not up to date, changing it"

        $rawContents = Get-Content $file -Raw

        $targetContent = $rawContents.Replace($contents.Trim(), $licenseFirstLine)

        $targetContent | Out-File $file -NoNewline
    }
}
