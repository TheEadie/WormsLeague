#!/bin/bash
GITHUB_TOKEN=$1
GITHUB_REPO=$2
RELEASE_ASSETS_FOLDER=$3
VERSION=$(cat $RELEASE_ASSETS_FOLDER/version.json | jq -r '.MajorMinorPatch')

CREATE_RELEASE_RESPONSE=$(curl --request POST \
    --url "https://api.github.com/repos/$GITHUB_REPO/releases" \
    --header "authorization: Bearer $GITHUB_TOKEN" \
    --header "content-type: application/json" \
    --data '{
                "tag_name": "schemes/v'$VERSION'",
                "target_commitish": "master",
                "name": "Redgate Schemes V'$VERSION'",
                "body": "",
                "draft": false,
                "prerelease": false }')

ASSETURL=$(echo $CREATE_RELEASE_RESPONSE | jq -r '.upload_url' | sed 's/{?name,label}//g')

for filename in $RELEASE_ASSETS_FOLDER/*; do
    echo "Uploading $filename"
    name="${filename##*/}"
    UPLOADURL=$ASSETURL?name=$name
    echo $UPLOADURL

    curl --request POST \
    --url $UPLOADURL \
    --header "authorization: Bearer $GITHUB_TOKEN" \
    --header "Content-Type: application/octet-stream" \
    --data-binary "@$filename"
done