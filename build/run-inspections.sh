#!/bin/bash
set -euo pipefail

>&2 echo "Running Rosyln inspections..."

mkdir -p .output
dotnet build Worms.sln /p:ErrorLog=$(pwd)/.output/roslyn.sarif /p:ErrorLogFormat=SarifV2 /p:TreatWarningsAsErrors=true /p:EnforceCodeStyleInBuild=true
