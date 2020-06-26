#!/bin/bash
source `dirname "$0"`/private/logging.sh

CleanArtifacts ()
{
    local OutputDir=$1

    WriteHeading "Cleaning..."
    WriteInfo "Cleaning $OutputDir/"
    rm $OutputDir -rf
}

Build ()
{
    local OutputDir=$1
    local Version_MajorMinorPatch=$2

    WriteHeading "Building Schemes v$Version_MajorMinorPatch..."

    WriteInfo "Writing version info to $OutputDir/version.json"
    mkdir $OutputDir
    echo $Version_Json > $OutputDir/version.json

    WriteInfo "Building"
    docker run -v $(pwd):/Schemes -v $(pwd)/$OutputDir:/.artifacts theeadie/wormscli create scheme "Uber.Coolest.Options.$Version_MajorMinorPatch" -f "/Schemes/Uber Coolest Options.txt" -r "/.artifacts"
    docker run -v $(pwd):/Schemes -v $(pwd)/$OutputDir:/.artifacts theeadie/wormscli create scheme "Speed.Worms.$Version_MajorMinorPatch" -f "/Schemes/Speed Worms.txt" -r "/.artifacts"

    WriteHighlight "Published to $OutputDir/"
}