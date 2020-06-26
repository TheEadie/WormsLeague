#!/bin/bash
source `dirname "$0"`/private/logging.sh

CreateGitHubRelease ()
{
    local Version=$1
    local GitHubToken=$2
    local GitHubRepo=$3
    local ReleaseDir=$4

    WriteHeading "Creating GitHub Release v$Version..."
    WriteVerbose "Calling - https://api.github.com/repos/$GitHubRepo/releases"

    CreateReleaseResponse=$(curl --request POST \
        --url "https://api.github.com/repos/$GitHubRepo/releases" \
        --header "authorization: Bearer $GitHubToken" \
        --header "content-type: application/json" \
        --data '{
                    "tag_name": "schemes/v'$Version'",
                    "target_commitish": "master",
                    "name": "Redgate Schemes v'$Version'",
                    "body": "",
                    "draft": false,
                    "prerelease": false }')

    WriteVerbose "$CreateReleaseResponse" # Print the message

    AssetUrl=$(echo $CreateReleaseResponse | jq -r '.upload_url' | sed 's/{?name,label}//g')

    for filename in $ReleaseDir/*; do
        WriteInfo "Uploading $filename"

        name="${filename##*/}"
        UploadUrl=$AssetUrl?name=$name
        WriteVerbose "Calling - $UploadUrl"

        curl --request POST \
        --url $UploadUrl \
        --header "authorization: Bearer $GitHubToken" \
        --header "Content-Type: application/octet-stream" \
        --data-binary "@$filename"
    done

    WriteHighlight "GitHub Release created"
}