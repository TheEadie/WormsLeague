#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/release-github.sh

GitHubToken=$1
GitHubRepo=$2
ReleaseDir=$3

GetVersionFromBuildArtifact
CreateGitHubRelease $Version_MajorMinorPatch $GitHubToken $GitHubRepo $ReleaseDir