#!/bin/bash
set -euo pipefail

ClientId=$1
ClientSecret=$2
OutputDir=$3
AuthServerUrl=https://eadie.eu.auth0.com/oauth/token
TokenAudience=worms.davideadie.dev

echo "Getting auth token..."

GetAuthTokenResponse=$(curl --request POST \
    --url "$AuthServerUrl" \
    --header 'content-type: application/json' \
    --data '{
        "client_id": "'"$ClientId"'",
        "client_secret": "'"$ClientSecret"'",
        "audience": "'"$TokenAudience"'",
        "grant_type": "client_credentials" }')

AuthToken=$(echo $GetAuthTokenResponse | jq -r '.access_token')

if [ "$AuthToken" = "null" ] || [ -z "$AuthToken" ]; then
    echo "Failed to get auth token. Response:"
    echo "$GetAuthTokenResponse"
    exit 1
fi

echo "Downloading WA game files..."

mkdir -p "$OutputDir"

HttpCode=$(curl --request GET \
    --url "https://worms.davideadie.dev/api/v1/files/game" \
    --header "authorization: Bearer $AuthToken" \
    --output "$OutputDir/wa-game.zip" \
    --write-out "%{http_code}" \
    -L)

if [ "$HttpCode" -ne 200 ]; then
    echo "Download failed with HTTP $HttpCode. Response body:"
    cat "$OutputDir/wa-game.zip"
    rm -f "$OutputDir/wa-game.zip"
    exit 1
fi

echo "Extracting WA game files..."

unzip -o "$OutputDir/wa-game.zip" -d "$OutputDir"
rm "$OutputDir/wa-game.zip"

echo "WA game files downloaded to $OutputDir"
