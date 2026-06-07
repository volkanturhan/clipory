# Builds the shareable ClipStack package: a single, self-contained Windows
# executable that runs without the .NET runtime installed.
#
# Output: dist/win-x64/ClipStack.exe  (~68 MB, compressed)
$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'ClipStack\ClipStack.csproj'
$output = Join-Path $root 'dist\win-x64'

dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $output

$exe = Join-Path $output 'ClipStack.exe'
Write-Output "Done: $exe ($([math]::Round((Get-Item $exe).Length / 1MB, 1)) MB)"
