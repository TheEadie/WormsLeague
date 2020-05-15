#!/bin/bash
source `dirname "$0"`/private/logging.sh
source `dirname "$0"`/private/calculate-version.sh

# Input
UseDocker=$1

CalculateVersion $UseDocker