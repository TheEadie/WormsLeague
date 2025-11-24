#!/bin/bash
set -euo pipefail

rm -rf sarif jetbrains.sarif roslyn.sarif

>&2 echo "Running Rosyln inspections..."
mkdir -p sarif
dotnet build Worms.sln /p:Sarif=true
dotnet tool install --local Sarif.Multitool
dotnet sarif merge sarif/*.sarif --recurse true --merge-runs --output-file=roslyn.sarif

>&2 echo "Running Jetbrains inspections..."
dotnet tool install --local JetBrains.ReSharper.GlobalTools
dotnet jb inspectcode Worms.sln --no-build --output=jetbrains.sarif --format=sarif
