#!/bin/bash
source `dirname "$0"`/private/logging.sh

BuildDockerImage ()
{
    local ImageName=$1
    local Version_Major=$2
    local Version_Minor=$3
    local Version_Patch=$4

    WriteHeading "Building Docker image..."

    WriteInfo "Building image $ImageName"
    docker build -t $ImageName:latest -f .build/dockerfile .
    docker tag $ImageName:latest $ImageName:$Version_Major
    WriteVerbose "Tagged $ImageName:$Version_Major"
    docker tag $ImageName:latest $ImageName:$Version_Major.$Version_Minor
    WriteVerbose "Tagged $ImageName:$Version_Major.$Version_Minor"
    docker tag $ImageName:latest $ImageName:$Version_Major.$Version_Minor.$Version_Patch
    WriteVerbose "Tagged $ImageName:$Version_Major.$Version_Minor.$Version_Patch"
}