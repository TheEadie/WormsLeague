#!/bin/bash
set -e
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/calculate-version.sh"
source "$SharedScripts/docker.sh"

# Input
ArtifactDir=$1
ImageName=$2

GetVersionFromBuildArtifact $ArtifactDir
BuildDockerImage $ImageName $Version_Major $Version_Minor $Version_Patch
