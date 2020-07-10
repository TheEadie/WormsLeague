#!/bin/bash
set -e
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/calculate-version.sh"
source "$SharedScripts/artifacts.sh"

# Input
UseDocker=$1
OutputDir=$2

# Build
CalculateVersion $UseDocker "Schemes"
CleanArtifacts $OutputDir

WriteHeading "Building Schemes v$Version_MajorMinorPatch..."

WriteInfo "Writing version info to $OutputDir/version.json"
mkdir $OutputDir
echo $Version_Json > $OutputDir/version.json

WriteInfo "Building"
docker run -v $(pwd):/Schemes -v $(pwd)/$OutputDir:/.artifacts theeadie/wormscli create scheme "Uber.Coolest.Options.$Version_MajorMinorPatch" -f "/Schemes/Uber Coolest Options.txt" -r "/.artifacts"
docker run -v $(pwd):/Schemes -v $(pwd)/$OutputDir:/.artifacts theeadie/wormscli create scheme "Speed.Worms.$Version_MajorMinorPatch" -f "/Schemes/Speed Worms.txt" -r "/.artifacts"

WriteHighlight "Published to $OutputDir/"