# Copyright (c) 2023 Sander van Vliet
# Licensed under Artistic License 2.0
# See LICENSE or https://choosealicense.com/licenses/artistic-2.0/
$csharpFiles = Get-ChildItem -Recurse -Filter *.cs | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "csharp-license.txt" -Raw

foreach($file in $csharpFiles)
{
    $contents = get-content $file -head 1
    if(!$contents.Trim().StartsWith("// Copyright (c) "))
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents
        Set-Content $file $targetContent
    }
}

$xamlFiles = Get-ChildItem -Recurse -Filter *.axaml | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "xaml-license.txt" -Raw

foreach($file in $xamlFiles)
{
    $contents = get-content $file -head 2
    if(!$contents[1].Trim().StartsWith("// Copyright (c) "))
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents
        Set-Content $file $targetContent
    }
}

$powershellFiles = Get-ChildItem -Recurse -Filter *.ps1 | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "powershell-license.txt" -Raw

foreach($file in $powershellFiles)
{
    $contents = get-content $file -head 1
    if(!$contents.Trim().StartsWith("# Copyright (c) "))
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents
        Set-Content $file $targetContent
    }
}
