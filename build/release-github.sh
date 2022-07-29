#!/bin/bash
ReleaseName=$1
Tag=$2
GitHubToken=$3
GitHubRepo=$4
ReleaseDir=$5

echo "Creating GitHub Release $ReleaseName..."
echo "Calling - https://api.github.com/repos/$GitHubRepo/releases"

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
                "prerelease": false,
                "generate_release_notes": true }')

echo "$CreateReleaseResponse" # Print the message

if [ -z "$ReleaseDir" ]; then
    # Nothing to upload
else
    AssetUrl=$(echo $CreateReleaseResponse | jq -r '.upload_url' | sed 's/{?name,label}//g')

    for filename in $ReleaseDir/*; do
        echo "Uploading $filename"

        name="${filename##*/}"
        UploadUrl=$AssetUrl?name=$name
        echo "Calling - $UploadUrl"

        curl --request POST \
        --url $UploadUrl \
        --header "authorization: Bearer $GitHubToken" \
        --header "Content-Type: application/octet-stream" \
        --data-binary "@$filename"
    done
fi

echo "GitHub Release created"