#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/docker.sh
source `dirname "$0"`/private/calculate-version.sh

# Input
DockerHubToken=$1
ReleaseDir=$2

echo "$ReleaseDir"

GetVersionFromBuildArtifact $ReleaseDir
BuildDockerImage "theeadie/wormscli" $Version_Major $Version_Minor $Version_Patch
PushDockerImages "theeadie" $DockerHubToken "theeadie/wormscli"
