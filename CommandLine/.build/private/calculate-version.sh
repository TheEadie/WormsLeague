#!/bin/bash
source `dirname "$0"`/private/logging.sh

# Input
DockerImage_GitVersion="gittools/gitversion:5.3.4-linux-alpine.3.10-x64-netcoreapp3.1"

GetVersionJson ()
{
    local UseDocker=$1

    if [ $UseDocker = "true" ]
    then
        WriteInfo "GitVersion: Using Docker image - $DockerImage_GitVersion"
        echo "$(docker run --rm -v "$(pwd)/..:/repo" $DockerImage_GitVersion /repo/CommandLine)"
    else
        WriteInfo "GitVersion: Using dotnet global tool"
        dotnet tool install GitVersion.Tool -g
        echo "$(dotnet gitversion)"
    fi
}

CalculateVersion ()
{
    local UseDocker=$1

    WriteHeading "Calculating version..."
    Version_Json=$(GetVersionJson $UseDocker)

    # Check if the returned value is json
    if !(jq -e . >/dev/null 2>&1 <<<"$Version_Json"); then
        WriteVerbose "$Version_Json" # This needs to be printed or any error output is lost
        WriteError "GitVersion: Failed to get version"
        exit
    fi

    Version_MajorMinorPatch=$(echo $Version_Json | jq -r '.MajorMinorPatch')
    WriteHighlight "Version - $Version_MajorMinorPatch"
    
    export Version_Json
    export Version_MajorMinorPatch
}

