#!/bin/bash
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
LIGHTWHITE='\033[0;97m'
LIGHTBLACK='\033[0;90m'
NOCOLOR='\033[0m'

WriteHeading ()
{
    WriteStdError "${BLUE}"
    WriteStdError "#################################"
    WriteStdError $1
    WriteStdError "#################################"
    WriteStdError "${NOCOLOR}"
}

WriteError ()
{
    WriteStdError "${RED}$1${NOCOLOR}"
}

WriteWarning ()
{
    WriteStdError "${YELLOW}$1${NOCOLOR}"
}

WriteHighlight ()
{
    WriteStdError "${GREEN}$1${NOCOLOR}"
}

WriteInfo ()
{
    WriteStdError "${LIGHTWHITE}$1${NOCOLOR}"
}

WriteVerbose ()
{
    WriteStdError "${LIGHTBLACK}$1${NOCOLOR}"
}

WriteStdError ()
{
    >&2 echo -e $@;
}
