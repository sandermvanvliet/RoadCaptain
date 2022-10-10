$csharpFiles = Get-ChildItem -Recurse -Filter *.cs | where-object {!$_.fullname.Contains("\obj\") -and !$_.fullname.Contains("\bin\")}

$prepend = Get-Content "csharp-license.txt" -Raw

foreach($file in $csharpFiles)
{
    $contents = get-content $file -head 1
    if($contents.Trim() -ne "// Copyright (c) 2022 Sander van Vliet")
    {
        $fullPath = $file.FullName

        Write-Host "$fullPath doesn't have a license header"

        $rawContents = Get-Content $file -Raw

        $targetContent = $prepend + $rawContents
        Set-Content $file $targetContent
    }
}