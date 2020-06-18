#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/calculate-version.sh
source `dirname "$0"`/private/publish.sh

# Input
UseDocker=$1
OutputDir=$2

CalculateVersion $UseDocker

WinOutputDir="$OutputDir/win-x64"
CleanArtifacts $WinOutputDir
Publish false "win-x64" $WinOutputDir $Version_MajorMinorPatch

LinuxOutputDir="$OutputDir/linux-x64"
CleanArtifacts $LinuxOutputDir
Publish false "linux-x64" $LinuxOutputDir $Version_MajorMinorPatch
