#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/docker.sh
source `dirname "$0"`/private/calculate-version.sh

# Input
DockerHubUsername=$1
DockerHubToken=$2
ReleaseDir=$3
ImageName=$4

GetVersionFromBuildArtifact $ReleaseDir
BuildDockerImage $ImageName $Version_Major $Version_Minor $Version_Patch
PushDockerImages $DockerHubUsername $DockerHubToken $ImageName
