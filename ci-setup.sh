#!/usr/bin/env bash

export PROJECT_PATH
PROJECT_PATH="$(pwd)/unity-renderer"

set -e
set -x
mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/
set +x

ls -lah /root/.cache/unity3d

echo "UNITY PATH is $UNITY_PATH"

./ci-setup-license.sh

set -x
