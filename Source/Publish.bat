@ECHO OFF

SET out=%~dp0\Publish

mkdir "%out%\temp"
dotnet publish -c Release -o "%out%"

cd %out%
move BnsLauncher.exe input.exe
ilrepack /out:"BnsLauncher.exe" /wildcards /parallel input.exe BnsLauncher.Core.dll Caliburn.Micro.Core.dll Caliburn.Micro.Platform.dll MaterialDesignColors.dll System.Threading.Tasks.Extensions.dll PropertyChanged.dll Unity.Abstractions.dll Unity.Container.dll System.Runtime.CompilerServices.Unsafe.dll Newtonsoft.Json.dll

move BnsLauncher.exe .\temp\
move FluentWPF.dll .\temp\
move MaterialDesignThemes.Wpf.dll .\temp\
move Microsoft.Xaml.Behaviors.dll .\temp\

del /Q *

cd temp
move * ../
cd ../
rmdir temp

copy ../../THIRD-PARTY-NOTICES.txt .