@echo off
setlocal enableextensions enabledelayedexpansion

set install_dir=target
set release_dir=deploy\bin\release
set msbuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

echo Building...

%msbuild% deploy.sln /t:Clean;Build /p:configuration=Release /verbosity:minimal

echo Merging...

packages\ILMerge.2.14.1208\tools\ilmerge.exe ^
	/target:exe ^
	/targetplatform:v4 ^
	/out:deploy.exe ^
	%release_dir%\deploy.exe ^
	%release_dir%\ICSharpCode.SharpZipLib.dll ^
	%release_dir%\Newtonsoft.Json.dll ^
	%release_dir%\Renci.SshNet.dll

for /f "tokens=*" %%x in ('cscript //nologo version.js deploy.exe') do set version=%%x

if not exist "%install_dir%" mkdir %install_dir%

echo Result: %install_dir% (%version%)

del deploy.pdb > nul
move deploy.exe %install_dir% > nul

endlocal
