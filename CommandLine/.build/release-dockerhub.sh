#!/bin/bash
set -e
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/docker.sh"
source "$SharedScripts/calculate-version.sh"

# Input
DockerHubUsername=$1
DockerHubToken=$2
ReleaseDir=$3
ImageName=$4

GetVersionFromBuildArtifact $ReleaseDir
BuildDockerImage $ImageName $Version_Major $Version_Minor $Version_Patch
PushDockerImages $DockerHubUsername $DockerHubToken $ImageName
