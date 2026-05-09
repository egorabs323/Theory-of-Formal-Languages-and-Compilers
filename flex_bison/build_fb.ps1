param(
    [string]$WinFlexPath = "$env:USERPROFILE\Downloads\win_flex_bison-latest\win_flex.exe",
    [string]$WinBisonPath = "$env:USERPROFILE\Downloads\win_flex_bison-latest\win_bison.exe",
    [string]$VsDevCmdPath = "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\Common7\Tools\VsDevCmd.bat"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

& $WinBisonPath -d -o fb_parser.tab.c fb_parser.y
& $WinFlexPath -o fb_lexer.yy.c fb_lexer.l

$cmd = "`"$VsDevCmdPath`" -arch=x64 && cl /nologo /O2 /W3 /utf-8 /D_CRT_SECURE_NO_WARNINGS /DYY_NO_UNISTD_H /Disatty=_isatty /Dfileno=_fileno fb_parser.tab.c fb_lexer.yy.c fb_runtime.c /Fe:fb_analyzer.exe"
cmd /c $cmd | Out-Host

Pop-Location
