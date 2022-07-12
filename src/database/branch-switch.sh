#!/bin/bash

SPAWN_IMAGE="worms-hub"

BRANCH_NAME="$(git rev-parse --symbolic-full-name --abbrev-ref HEAD)"
CONTAINER_NAME="branch.$BRANCH_NAME"


pkill -f spawnctl

if ! spawnctl get data-container "$CONTAINER_NAME" &> /dev/null ; then
    spawnctl create dc --name "$CONTAINER_NAME" --image "$SPAWN_IMAGE" --lifetime 24h
fi

if ! spawnctl get data-container shadow &> /dev/null ; then
    spawnctl create dc --name shadow --image "$SPAWN_IMAGE" --lifetime 24h
fi

spawnctl proxy dc $CONTAINER_NAME &> /dev/null &
spawnctl proxy dc shadow --port 5433 &> /dev/null &