#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
source "$ScriptDir/logging.sh"

# Input
DockerImage_DotnetSdk="mcr.microsoft.com/dotnet/core/sdk:3.1.8"

Dotnet-Publish ()
{
    local UseDocker=$1
    local OutputDir=$2
    local Version_MajorMinorPatch=$3
    local Runtime=$4

    if [ $UseDocker = "true" ]
    then
        WriteInfo "dotnet publish: Using Docker image - $DockerImage_DotnetSdk"
        docker run -t \
        -v `pwd`:/repo \
        -v `pwd`/$OutputDir:/$OutputDir \
        -w /repo \
        $DockerImage_DotnetSdk \
            dotnet publish \
            -c Release \
            -r $Runtime \
            -o $OutputDir \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$Version_MajorMinorPatch \
            /p:PublishTrimmed=true \
            /p:DebugType=none
    else
        WriteInfo "dotnet publish: Using local install"
        dotnet publish \
            -c Release \
            -r $Runtime \
            -o $OutputDir \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$Version_MajorMinorPatch \
            /p:PublishTrimmed=true \
            /p:DebugType=none
    fi
}