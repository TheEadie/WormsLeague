#!/bin/bash
set -euo pipefail

>&2 echo "Running Rosyln inspections..."

mkdir -p sarif
dotnet build Worms.sln /p:Sarif=true

>&2 echo "Running Jetbrains inspections..."

dotnet tool install --local JetBrains.ReSharper.GlobalTools
dotnet jb inspectcode Worms.sln --output=jetbrains.sarif --format=sarif
