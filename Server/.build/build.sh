#!/bin/bash
set -e
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/calculate-version.sh"
source "$SharedScripts/docker.sh"

# Input
UseDocker=$1
ImageName=$2

#Build
CalculateVersion "$UseDocker" "Server"
BuildDockerImage $ImageName $Version_Major $Version_Minor $Version_Patch