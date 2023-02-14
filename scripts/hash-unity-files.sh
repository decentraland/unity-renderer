#! /usr/bin/env bash

set -e

# hash unity files
find ./unity-renderer -type f \
    \( -not -path '*Library*' \) \
    \( -not -path '*browser-interface*' \) \
    \( -not -path '*node_modules*' \) \
    \( -not -path '*Sentry/SentryOptions.asset' \) \
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
    \( -not -path '*unity-renderer*' \) \
    \( -not -path '*browser-interface*' \) \
    \( -iname \*.sh \
    -o -iname \*.yml \) \
    \( -exec md5sum "$PWD"/{} \; \) |
    sort >>../.unitysources-checksum

# print the result
# disabled -- feel free to enable it to debug the pipeline
# cat ../.unitysources-checksum
