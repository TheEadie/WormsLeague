#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
source "$ScriptDir/logging.sh"

CreateGitHubRelease ()
{
    local ReleaseName=$1
    local Tag=$2
    local GitHubToken=$3
    local GitHubRepo=$4
    local ReleaseDir=$5

    WriteHeading "Creating GitHub Release $ReleaseName..."
    WriteVerbose "Calling - https://api.github.com/repos/$GitHubRepo/releases"

    CreateReleaseResponse=$(curl --request POST \
        --url "https://api.github.com/repos/$GitHubRepo/releases" \
        --header "authorization: Bearer $GitHubToken" \
        --header "content-type: application/json" \
        --data '{
                    "tag_name": "'"$Tag"'",
                    "target_commitish": "master",
                    "name": "'"$ReleaseName"'",
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