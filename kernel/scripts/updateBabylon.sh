set -e
set -i

VERSION=$1

CODE='.dependencies | keys | map(select(. | test("babylonjs"))) | map(. + "@latest") | join(" ")'

COMMANDS="npm install -S $(jq -r "${CODE}" < package.json)"

COMMANDS=${COMMANDS//latest/$VERSION}

eval "${COMMANDS}"

