#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/calculate-version.sh
source `dirname "$0"`/private/publish.sh

# Input
UseDocker=$1
OutputDir=$2
Platform=$3

PlatformOutputDir="$OutputDir/$Platform"

CalculateVersion $UseDocker
CleanArtifacts $PlatformOutputDir
Publish false $Platform $PlatformOutputDir $Version_MajorMinorPatch