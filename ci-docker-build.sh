#!/bin/bash

set -u # fail if env vars are unbound

docker run -it --rm \
  -e "BUILD_TARGET=WebGL" \
  -e "BUILD_PATH=/app/unity-renderer/Builds/unity" \
  -e "PROJECT_PATH=/app/unity-renderer/" \
  -e "BUILD_NAME=unity" \
  -e "DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64=${DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64}" \
  --memory=16gb \
  --cpus=4 \
  -w "/app" \
  -v "$(pwd):/app" \
  unity \
  bash