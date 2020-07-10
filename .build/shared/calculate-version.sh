#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
source "$ScriptDir/logging.sh"

# Input
DockerImage_GitVersion="gittools/gitversion:5.3.4-linux-alpine.3.10-x64-netcoreapp3.1"

GetVersionJson ()
{
    local UseDocker=$1
    local SubFolder=$2

    if [ $UseDocker = "true" ]
    then
        WriteInfo "GitVersion: Using Docker image - $DockerImage_GitVersion"
        echo "$(docker run --rm -v "$(pwd)/..:/repo" $DockerImage_GitVersion /repo/$SubFolder)"
    else
        WriteInfo "GitVersion: Using dotnet global tool"
        dotnet tool install GitVersion.Tool -g
        echo "$(dotnet gitversion)"
    fi
}

CalculateVersion ()
{
    local UseDocker=$1
    local SubFolder=$2

    WriteHeading "Calculating version..."
    Version_Json=$(GetVersionJson "$UseDocker" "$SubFolder")

    # Check if the returned value is json
    if !(jq -e . >/dev/null 2>&1 <<<"$Version_Json"); then
        WriteVerbose "$Version_Json" # This needs to be printed or any error output is lost
        WriteError "GitVersion: Failed to get version"
        exit
    fi

    Version_MajorMinorPatch=$(echo $Version_Json | jq -r '.MajorMinorPatch')
    Version_Major=$(echo $Version_Json | jq -r '.Major')
    Version_Minor=$(echo $Version_Json | jq -r '.Minor')
    Version_Patch=$(echo $Version_Json | jq -r '.Patch')
    WriteHighlight "Version - $Version_MajorMinorPatch"

    export Version_Json
    export Version_MajorMinorPatch
    export Version_Major
    export Version_Minor
    export Version_Patch
}

GetVersionFromBuildArtifact ()
{
    local ArtifactDir=$1

    WriteHeading "Getting version from build artifact..."
    Version_Json=$(cat $ArtifactDir/version.json)

    # Check if the returned value is json
    if !(jq -e . >/dev/null 2>&1 <<<"$Version_Json"); then
        WriteVerbose "$Version_Json" # This needs to be printed or any error output is lost
        WriteError "Failed to get version from build artifact"
        exit
    fi

    Version_MajorMinorPatch=$(echo $Version_Json | jq -r '.MajorMinorPatch')
    Version_Major=$(echo $Version_Json | jq -r '.Major')
    Version_Minor=$(echo $Version_Json | jq -r '.Minor')
    Version_Patch=$(echo $Version_Json | jq -r '.Patch')
    WriteHighlight "Version - $Version_MajorMinorPatch"

    export Version_Json
    export Version_MajorMinorPatch
    export Version_Major
    export Version_Minor
    export Version_Patch
}