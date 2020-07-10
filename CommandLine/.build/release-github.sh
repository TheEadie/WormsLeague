#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
SharedScripts="$ScriptDir/../../.build/shared"
source "$SharedScripts/logging.sh"
source "$SharedScripts/release-github.sh"
source "$SharedScripts/calculate-version.sh"

# Input
GitHubToken=$1
GitHubRepo=$2
ReleaseDir=$3

GetVersionFromBuildArtifact $ReleaseDir
CreateGitHubRelease "CLI v$Version_MajorMinorPatch" "cli/v$Version_MajorMinorPatch" $GitHubToken $GitHubRepo $ReleaseDir
