@echo off
setlocal

call "%~dp0Build.cmd" Release

NuGet.exe pack .\SemVerHarvester.nuspec
