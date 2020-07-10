#!/bin/bash
set -e
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
CreateGitHubRelease "Redgate Schemes v$Version_MajorMinorPatch" "schemes/v$Version_MajorMinorPatch" "$GitHubToken" "$GitHubRepo" "$ReleaseDir"
