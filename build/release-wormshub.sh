#!/bin/bash
set -euo pipefail

ClientId=$1
ClientSecret=$2
AuthServerUrl=https://eadie.eu.auth0.com/oauth/token
TokenAudience=worms.davideadie.dev

echo "Compressing artifacts..."

zip -r .artifacts/worms-cli-windows.zip ./.artifacts/win-x64/*
tar -czvf .artifacts/worms-cli-linux.tar.gz ./.artifacts/linux-x64/*

echo "Getting auth token..."

GetAuthTokenResponse=$(curl --request POST \
    --url ""$AuthServerUrl"" \
    --header 'content-type: application/json' \
    --data '{
        "client_id": "'"$ClientId"'",
        "client_secret": "'"$ClientSecret"'",
        "audience": "'"$TokenAudience"'",
        "grant_type": "client_credentials" }')

AuthToken=$(echo $GetAuthTokenResponse | jq -r '.access_token')

echo "Uploading Windows release..."

curl --request POST \
    --url "https://worms.davideadie.dev/api/v1/files/cli" \
    --header "authorization: Bearer $AuthToken" \
    --form "file=@".artifacts/worms-cli-windows.zip"" \
    --form "platform="windows"" \
    --fail-with-body

echo "Uploading Linux release..."

curl --request POST \
    --url "https://worms.davideadie.dev/api/v1/files/cli" \
    --header "authorization: Bearer $AuthToken" \
    --form "file=@".artifacts/worms-cli-linux.tar.gz"" \
    --form "platform="linux"" \
    --fail-with-body
