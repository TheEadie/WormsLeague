#!/bin/bash

SPAWN_IMAGE="worms-hub"

BRANCH_NAME="$(git rev-parse --symbolic-full-name --abbrev-ref HEAD)"
CONTAINER_NAME="branch.$BRANCH_NAME"


pkill -f spawnctl

if ! spawnctl get data-container "$CONTAINER_NAME" &> /dev/null ; then
    spawnctl create dc --name "$CONTAINER_NAME" --image "$SPAWN_IMAGE"
fi

spawnctl proxy dc $CONTAINER_NAME &> /dev/null &