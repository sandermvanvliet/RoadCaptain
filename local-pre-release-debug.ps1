# Copyright (c) 2025 Sander van Vliet
# Licensed under Artistic License 2.0
# See LICENSE or https://choosealicense.com/licenses/artistic-2.0/
$RID="win-x64"

dotnet restore -r $RID

cp src/RoadCaptain.App.Runner/appsettings.routerepositories.release.json src/RoadCaptain.App.Runner/appsettings.routerepositories.json
cp src/RoadCaptain.App.RouteBuilder/appsettings.routerepositories.release.json src/RoadCaptain.App.RouteBuilder/appsettings.routerepositories.json

dotnet test --verbosity minimal -c Debug -r $RID -p:RuntimeIdentifier=$RID -p:RuntimeIdentifiers=$RID

dotnet publish --self-contained -c Debug -r $RID -p:RuntimeIdentifiers=$RID -p:PreRelease=True src\RoadCaptain.App.Runner\RoadCaptain.App.Runner.csproj

dotnet publish --self-contained -c Debug -r $RID -p:RuntimeIdentifiers=$RID -p:PreRelease=True src\RoadCaptain.App.RouteBuilder\RoadCaptain.App.RouteBuilder.csproj

dotnet run --project .\packaging\RoadCaptain.WixComponentFileGenerator\RoadCaptain.WixComponentFileGenerator.csproj src\RoadCaptain.App.Runner\bin\Debug\net6.0-windows\$RID\publish src\RoadCaptain.App.RouteBuilder\bin\Debug\net6.0-windows\$RID\publish .\packaging\RoadCaptain.Installer\Components.wxi

msbuild .\packaging\RoadCaptain.Installer\RoadCaptain.Installer.wixproj -property:Configuration=Debug -property:RunnerTargetDir=C:\git\RoadCaptain\src\RoadCaptain.App.Runner\bin\Debug\net6.0-windows\$RID\publish\ -property:RouteBuilderTargetDir=C:\git\RoadCaptain\src\RoadCaptain.App.RouteBuilder\bin\Debug\net6.0-windows\$RID\publish\ -property:PreRelease=True
