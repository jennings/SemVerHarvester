@echo off
setlocal

call "%~dp0Build.cmd" Release

NuGet.exe pack .\SemVerParser.nuspec