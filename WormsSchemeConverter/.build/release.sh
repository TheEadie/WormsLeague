#!/bin/bash
GITHUB_TOKEN=$1
GITHUB_REPO=$2
RELEASE_NAME=$3
RELEASE_BODY=$4
RELEASE_ASSETS_FOLDER=$5

CREATE_RELEASE_RESPONSE=$(curl --request POST \
    --url "https://api.github.com/repos/$GITHUB_REPO/releases" \
    --header "authorization: Bearer $GITHUB_TOKEN" \
    --header "content-type: application/json" \
    --data '{
                "tag_name": "'$RELEASE_NAME'",
                "target_commitish": "master",
                "name": "'$RELEASE_NAME'",
                "body": "'$RELEASE_BODY'",
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
    --data-binary "$filename"
done