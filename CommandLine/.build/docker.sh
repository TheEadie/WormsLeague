#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/calculate-version.sh
source `dirname "$0"`/private/docker.sh

# Input
ArtifactDir=$1

GetVersionFromBuildArtifact $ArtifactDir
BuildDockerImage "theeadie/wormscli" $Version_Major $Version_Minor $Version_Patch
