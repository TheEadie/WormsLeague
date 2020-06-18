#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/dotnet.sh

CleanArtifacts ()
{
    local OutputDir=$1

    WriteHeading "Cleaning..."
    WriteInfo "Cleaning $OutputDir/"
    rm $OutputDir -rf
}

Publish ()
{
    local UseDocker=$1
    local Platform=$2
    local OutputDir=$3
    local Version_MajorMinorPatch=$4

    WriteHeading "Publishing $Platform v$Version_MajorMinorPatch..."

    WriteInfo "Building $Platform"
    Dotnet-Publish $UseDocker $OutputDir $Version_MajorMinorPatch $Platform

    WriteInfo "Writing install script to output"
    cp *.ps1 $OutputDir

    WriteInfo "Writing version info to $OutputDir/version.json"
    echo $Version_Json > $OutputDir/version.json

    WriteHighlight "Published to $OutputDir/"
}