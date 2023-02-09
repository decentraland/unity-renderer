#! /usr/bin/env bash

set -e

# hash unity files
find ./unity-renderer -type f \
    \( -not -path '*Library*' \) \
    \( -not -path '*browser-interface*' \) \
    \( -not -path '*node_modules*' \) \
    \( -iname \*.unity \
    -o -iname \*.sh \
    -o -iname \*.cs \
    -o -iname \*.meta \
    -o -iname \*.xml \
    -o -iname \*.shader \
    -o -iname \*.prefab \
    -o -iname \*.yml \
    -o -iname \*.mat \
    -o -iname \*.json \
    -o -iname \*.js \
    -o -iname \*.jspre \
    -o -iname \*.jslib \
    -o -iname \*.hlsl \
    -o -iname \*.asmdef \
    -o -iname \*.csproj \
    -o -iname \*.spriteatlas \
    -o -iname \*.asset \) \
    \( -exec md5sum "$PWD"/{} \; \) |
    sort >../.unitysources-checksum

# hash pipeline files
find ./ -type f \
    \( -not -path '*node_modules*' \) \
    \( -iname \*.sh \ -o -iname \*.yml \ \) \
    \( -exec md5sum "$PWD"/{} \; \) |
    sort >> ../.unitysources-checksum

# print the result
cat ../.unitysources-checksum