#!/bin/bash
set -euo pipefail

PATHTOJSON=$1

NUMOFADDITIONS=$(jq '.individualResults[] | select(.operation=="changes") | .onlyInTarget | length' "$PATHTOJSON" -r)
NUMOFDELETIONS=$(jq '.individualResults[] | select(.operation=="changes") | .onlyInSource | length' "$PATHTOJSON" -r)
NUMOFCHANGES=$(jq '.individualResults[] | select(.operation=="changes") | .differences | length' "$PATHTOJSON" -r)

printf "### Database Changes\n"
printf "✅ Objects to be created: **%s**\n" "$NUMOFADDITIONS"
if [ "$NUMOFADDITIONS" -gt 0 ]; then
    mapfile -t ADDITIONS < <(jq '.individualResults[] | select(.operation=="changes").onlyInTarget[] | ("  - ✅ " + .objectType + " - " + .schema + "." + .name)' "$PATHTOJSON" -r)
    printf "%s\n" "${ADDITIONS[@]}"
fi

printf "\n📝 Objects to be updated: **%s**\n" "$NUMOFCHANGES"
if [ "$NUMOFCHANGES" -gt 0 ]; then
    mapfile -t CHANGES < <(jq '.individualResults[] | select(.operation=="changes").differences[] | ("  - 📝 " + .objectType + " - " + .schema + "." + .name)' "$PATHTOJSON" -r)
    printf "%s\n" "${CHANGES[@]}"
fi

printf "\n⚠ Objects to be dropped: **%s**\n" "$NUMOFDELETIONS"
if [ "$NUMOFDELETIONS" -gt 0 ]; then
    mapfile -t DELETIONS < <(jq '.individualResults[] | select(.operation=="changes").onlyInSource[] | ("  - ⚠ " + .objectType + " - " + .schema + "." + .name)' "$PATHTOJSON" -r)
    printf "%s\n" "${DELETIONS[@]}"
fi
