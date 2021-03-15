#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
source "$ScriptDir/logging.sh"

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

PushDockerImages ()
{
    local Username=$1
    local Password=$2
    local ImageName=$3

    WriteHeading "Pushing Docker image..."
    echo $Password | docker login -u $Username --password-stdin
    docker push $ImageName --all-tags
}
