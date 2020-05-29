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
    local OutputDir=$2
    local Version_MajorMinorPatch=$3

    WriteHeading "Publishing version - $Version_MajorMinorPatch..."

    WriteInfo "Making output directory $OutputDir/"
    mkdir .artifacts

    WriteInfo "Writing version info to $OutputDir/version.json"
    echo $Version_Json > $OutputDir/version.json

    WriteInfo "Building win-x64"
    Dotnet-Publish $UseDocker $OutputDir $Version_MajorMinorPatch win-x64

    WriteInfo "Building linux-x64"
    Dotnet-Publish $UseDocker $OutputDir $Version_MajorMinorPatch linux-x64

    WriteInfo "Writing install script to output"
    cp *.ps1 $OutputDir

    WriteHighlight "Published to $OutputDir/"
}