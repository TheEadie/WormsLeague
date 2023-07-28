#!/bin/bash 

PATHTOJSON=$1

function join_by {
  local d=${1-} f=${2-}
  if shift 1; then
    printf %s "${@/#/$d}"
  fi
}

NUMOFADDITIONS=$(jq '.individualResults[] | select(.operation=="changes") | .onlyInTarget | length' $PATHTOJSON -r)
NUMOFDELETIONS=$(jq '.individualResults[] | select(.operation=="changes") | .onlyInSource | length' $PATHTOJSON -r)
NUMOFCHANGES=$(jq '.individualResults[] | select(.operation=="changes") | .differences | length' $PATHTOJSON -r)


printf "<h3>Database Changes</h3>"
printf "✅ Objects to be created: <b>"$NUMOFADDITIONS"</b>"

if [ "$NUMOFADDITIONS" -gt 0 ]; then
    ADDITIONS=$(jq '.individualResults[] | select(.operation=="changes").onlyInTarget[] | (.objectType + "&nbsp;-&nbsp;" + .schema + "." + .name) ' $PATHTOJSON -r)
    join_by "<br/>&nbsp;| ✅ " $ADDITIONS
fi

printf "<br/>📝 Objects to be updated: <b>"$NUMOFCHANGES"</b>"
if [ "$NUMOFCHANGES" -gt 0 ]; then
    CHANGES=$(jq '.individualResults[] | select(.operation=="changes").differences[] | (.objectType + "&nbsp;-&nbsp;" + .schema + "." + .name) ' $PATHTOJSON -r)
    join_by "<br/>&nbsp;| 📝 " $CHANGES
fi

printf "<br/>⚠ Objects to be dropped: <b>"$NUMOFDELETIONS"</b>"
if [ "$NUMOFDELETIONS" -gt 0 ]; then
    DELETIONS=$(jq '.individualResults[] | select(.operation=="changes").onlyInSource[] | (.objectType + "&nbsp;-&nbsp;" + .schema + "." + .name) ' $PATHTOJSON -r)
    join_by "<br/>&nbsp;| ⚠ " $DELETIONS
fi


