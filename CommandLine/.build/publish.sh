#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/calculate-version.sh
source `dirname "$0"`/private/publish.sh

# Input
UseDocker=$1
OutputDir=$2

CleanArtifacts $OutputDir
CalculateVersion $UseDocker
Publish false $OutputDir $Version_MajorMinorPatch
