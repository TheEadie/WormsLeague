#!/bin/bash
set -euo pipefail

>&2 echo "Running Rosyln inspections..."

mkdir -p src/sarif
dotnet build Worms.sln /p:Sarif=true
