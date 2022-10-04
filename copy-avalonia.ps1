$dlls = Get-ChildItem -Filter Avalonia*.dll "C:\git\Avalonia\samples\ControlCatalog.Desktop\bin\Debug\net461"

$dllTargetDir = "c:\git\RoadCaptain\vendor\AvaloniaUI"
$propsFile = "c:\git\RoadCaptain\vendor\AvaloniaUI\AvaloniaLocal.props"

"<Project>" > $propsFile
"  <ItemGroup>" >> $propsFile

foreach($item in $dlls)
{
    $path = $item.FullName
    $pdbPath = $path.Replace(".dll", ".pdb")

    copy-item $path $dllTargetDir
    copy-item $pdbPath $dllTargetDir

    $dllName = $item.Name
     "    <Reference Include=`"$dllName`"><HintPath>..\..\vendor\AvaloniaUI\$dllName</HintPath></Reference>" >> $propsFile
}

copy-item "C:\git\Avalonia\samples\ControlCatalog.Desktop\bin\Debug\net461\SkiaSharp.dll" $dllTargetDir
copy-item "C:\git\Avalonia\samples\ControlCatalog.Desktop\bin\Debug\net461\libSkiaSharp.dll" $dllTargetDir
"    <Reference Include=`"SkiaSharp.dll`"><HintPath>..\..\vendor\AvaloniaUI\SkiaSharp.dll</HintPath></Reference>" >> $propsFile

copy-item "C:\git\Avalonia\samples\ControlCatalog.Desktop\bin\Debug\net461\libHarfBuzzSharp.dll" $dllTargetDir
copy-item "C:\git\Avalonia\samples\ControlCatalog.Desktop\bin\Debug\net461\HarfBuzzSharp.dll" $dllTargetDir
"    <Reference Include=`"HarfBuzzSharp.dll`"><HintPath>..\..\vendor\AvaloniaUI\HarfBuzzSharp.dll</HintPath></Reference>" >> $propsFile

copy-item "C:\git\Avalonia\src\Avalonia.ReactiveUI\bin\Debug\netstandard2.0\Avalonia.ReactiveUI.dll" $dllTargetDir
copy-item "C:\git\Avalonia\src\Avalonia.ReactiveUI\bin\Debug\netstandard2.0\Avalonia.ReactiveUI.pdb" $dllTargetDir
$dllName = "Avalonia.ReactiveUI.dll"
"    <Reference Include=`"$dllName`"><HintPath>..\..\vendor\AvaloniaUI\$dllName</HintPath></Reference>" >> $propsFile

"  </ItemGroup>" >> $propsFile
"</Project>" >> $propsFile

copy-item "C:\git\Avalonia\packages\Avalonia\*.props" $dllTargetDir
copy-item "C:\git\Avalonia\packages\Avalonia\*.targets" $dllTargetDir

$dlls = Get-ChildItem -Filter *.dll "C:\git\Avalonia\src\Avalonia.Build.Tasks\bin\Debug\netstandard2.0"
foreach($item in $dlls)
{
    $fileName = $item.Name
    $targetFileName = "$dllTargetDir\$fileName"

    if(!(Test-Path $targetFileName))
    {
        copy-item -Force $item $targetFileName
    }
}