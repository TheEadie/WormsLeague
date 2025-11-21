#!/bin/bash
set -euo pipefail

>&2 echo "Running Rosyln inspections..."

mkdir -p sarif
dotnet build Worms.sln /p:Sarif=true
