#!/bin/bash
source `dirname "$0"`/private/logging.sh

GetVersionFromBuildArtifact ()
{
    WriteHeading "Getting version from build artifact..."
    Version_Json=$(cat $ReleaseDir/version.json)

    # Check if the returned value is json
    if !(jq -e . >/dev/null 2>&1 <<<"$Version_Json"); then
        WriteVerbose "$Version_Json" # This needs to be printed or any error output is lost
        WriteError "Failed to get version from build artifact"
        exit
    fi

    Version_MajorMinorPatch=$(echo $Version_Json | jq -r '.MajorMinorPatch')
    WriteHighlight "Version - $Version_MajorMinorPatch"

    export Version_Json
    export Version_MajorMinorPatch
}

CreateGitHubRelease ()
{
    local Version=$1
    local GitHubToken=$2
    local GitHubRepo=$3
    local ReleaseDir=$4

    WriteHeading "Creating GitHub Release v$Version..."

    CreateReleaseResponse=$(curl --request POST \
        --url "https://api.github.com/repos/$GitHubRepo/releases" \
        --header "authorization: Bearer $GitHubToken" \
        --header "content-type: application/json" \
        --data '{
                    "tag_name": "cli/v'$Version'",
                    "target_commitish": "master",
                    "name": "CLI v'$Version'",
                    "body": "",
                    "draft": false,
                    "prerelease": false }')

    # Check if the returned value has an error message
    if !(jq -r '.message' . >/dev/null 2>&1 <<<"$CreateReleaseResponse"); then
        WriteVerbose "$CreateReleaseResponse" # Print the message
        WriteError "Failed to create GitHub Release"
        exit
    fi

    AssetUrl=$(echo $CreateReleaseResponse | jq -r '.upload_url' | sed 's/{?name,label}//g')

    for filename in $ReleaseDir/*; do
        WriteInfo "Uploading $filename"

        name="${filename##*/}"
        UploadUrl=$AssetUrl?name=$name
        WriteVerbose "URL - $UploadUrl"

        curl --request POST \
        --url $UploadUrl \
        --header "authorization: Bearer $GitHubToken" \
        --header "Content-Type: application/octet-stream" \
        --data-binary "@$filename"
    done

    WriteHighlight "GitHub Release created"
}