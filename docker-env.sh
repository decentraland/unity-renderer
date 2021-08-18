#!/bin/bash

docker run -it \
  -e "BUILD_TARGET=WebGL" \
  -e "BUILD_PATH=/project/Builds/unity" \
  -e "BUILD_NAME=unity" \
  -e "UNITY_LICENSE_2019_4=${UNITY_LICENSE_2019_4}" \
  -w "/project" \
  -v "$(pwd):/project" \
  decentraland/renderer-build:2019.4 \
  bash