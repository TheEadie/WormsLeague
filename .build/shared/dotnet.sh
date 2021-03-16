#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
source "$ScriptDir/logging.sh"

# Input
DockerImage_DotnetSdk="mcr.microsoft.com/dotnet/core/sdk:3.1.9"

Dotnet-Publish ()
{
    local UseDocker=$1
    local ProjectPath=$2
    local OutputDir=$3
    local Version_MajorMinorPatch=$4
    local Runtime=$5

    if [ $UseDocker = "true" ]
    then
        WriteInfo "dotnet publish: Using Docker image - $DockerImage_DotnetSdk"
        docker run -t \
        -v `pwd`:/repo \
        -v `pwd`/$OutputDir:/$OutputDir \
        -w /repo \
        $DockerImage_DotnetSdk \
            dotnet publish \
            $ProjectPath \
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
            $ProjectPath \
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