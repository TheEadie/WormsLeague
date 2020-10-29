#!/bin/bash
set -e
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/calculate-version.sh"

# Input
UseDocker=$1

CalculateVersion "$UseDocker" "Server"