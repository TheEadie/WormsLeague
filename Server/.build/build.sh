#!/bin/bash
set -e
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/calculate-version.sh"
source "$SharedScripts/artifacts.sh"
source "$SharedScripts/dotnet.sh"

# Input
UseDocker=$1
OutputDir=$2
Platform=$3

#Build
PlatformOutputDir="$OutputDir/$Platform"

CalculateVersion "$UseDocker" "Server"
CleanArtifacts "$PlatformOutputDir"

WriteHeading "Publishing $Platform v$Version_MajorMinorPatch..."

WriteInfo "Building $Platform"
Dotnet-Publish false "src/Worms.Gateway/Worms.Gateway.csproj" $PlatformOutputDir $Version_MajorMinorPatch $Platform

WriteInfo "Writing version info to $PlatformOutputDir/version.json"
echo $Version_Json > $PlatformOutputDir/version.json

WriteHighlight "Published to $PlatformOutputDir/"