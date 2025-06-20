#!/bin/bash
set -euo pipefail

NEXT_VERSION="$1"
TAG_PREFIX="$2"

# Read Version parts
IFS='.'  
read -r -a VERSION_PARTS <<<"$NEXT_VERSION"
  
MAJOR="${VERSION_PARTS[0]}"
MINOR="${VERSION_PARTS[1]}"
PATCH=0

# Get last tag
LATEST_TAG=$(git describe --tags --match "$TAG_PREFIX*" --abbrev=0 2>/dev/null)
HEAD_HASH=$(git rev-parse --verify HEAD)
TAG_HASH=$(git log -1 --format=format:"%H" "$LATEST_TAG" 2>/dev/null | tail -n1)

if [[ -z "$LATEST_TAG" ]]; then
    >&2 echo "No previous versions found"
    >&2 echo "Calculated version: $NEXT_VERSION.0"
    echo "$NEXT_VERSION.0"
    exit
fi

if [[ -n "$TAG_PREFIX" ]]; then
    LATEST_TAG=${LATEST_TAG#"$TAG_PREFIX"}
fi

if [[ "$HEAD_HASH" == "$TAG_HASH" ]]; then
    >&2 echo "No changes since previous version"
    >&2 echo "Calculated version: $LATEST_TAG"
    echo "$LATEST_TAG"
    exit
fi

# Read Version parts
IFS='.'  
read -r -a VERSION_PARTS <<<"$LATEST_TAG"

LATEST_MAJOR="${VERSION_PARTS[0]}"
LATEST_MINOR="${VERSION_PARTS[1]}"
LATEST_PATCH="${VERSION_PARTS[2]}"

if [[ "$MAJOR" > "$LATEST_MAJOR" ]]; then
    >&2 echo "Calculated version: ${MAJOR}.${MINOR}.${PATCH}"
    echo "${MAJOR}.${MINOR}.${PATCH}"
    exit
elif [[ "$MINOR" > "$LATEST_MINOR" ]]; then
    >&2 echo "Calculated version: ${MAJOR}.${MINOR}.${PATCH}"
    echo "${MAJOR}.${MINOR}.${PATCH}"
    exit
else
    ((LATEST_PATCH++))
    >&2 echo "Calculated version: ${MAJOR}.${MINOR}.${LATEST_PATCH}"
    echo "${MAJOR}.${MINOR}.${LATEST_PATCH}"
    exit
fi