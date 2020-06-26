#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/calculate-version.sh
source `dirname "$0"`/private/build.sh

# Input
UseDocker=$1
OutputDir=$2

CalculateVersion $UseDocker
CleanArtifacts $OutputDir
Build $OutputDir $Version_MajorMinorPatch