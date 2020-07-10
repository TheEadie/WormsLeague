#!/bin/bash
ScriptDir="${BASH_SOURCE%/*}"
source "$ScriptDir/logging.sh"

CleanArtifacts ()
{
    local OutputDir=$1

    WriteHeading "Cleaning..."
    WriteInfo "Cleaning $OutputDir/"
    rm $OutputDir -rf
}