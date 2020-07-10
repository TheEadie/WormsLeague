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

CalculateVersion "$UseDocker" "CommandLine"
CleanArtifacts "$PlatformOutputDir"

WriteHeading "Publishing $Platform v$Version_MajorMinorPatch..."

WriteInfo "Building $Platform"
Dotnet-Publish false $PlatformOutputDir $Version_MajorMinorPatch $Platform

WriteInfo "Writing install script to output"
cp *.ps1 $PlatformOutputDir

WriteInfo "Writing version info to $PlatformOutputDir/version.json"
echo $Version_Json > $PlatformOutputDir/version.json

WriteHighlight "Published to $PlatformOutputDir/"