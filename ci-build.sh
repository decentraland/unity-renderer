#!/usr/bin/env bash

set -x

echo "Building for $BUILD_TARGET"

export UNITY_DIR="$(pwd)"
export BUILD_PATH="$UNITY_DIR/Builds/$BUILD_NAME/"
mkdir -p "$BUILD_PATH"


${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity } \
  -quit \
  -batchmode \
  -projectPath "$UNITY_DIR" \
  -buildTarget "$BUILD_TARGET" \
  -customBuildTarget "$BUILD_TARGET" \
  -customBuildName "$BUILD_NAME" \
  -customBuildPath "$BUILD_PATH" \
  -customBuildOptions AcceptExternalModificationsToPlayer \
  -executeMethod BuildCommand.PerformBuild \
  -manualLicenseFile /root/.local/share/unity3d/Unity/Unity_lic.ulf \
  -logFile /dev/stdout

find "$BUILD_PATH"

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

ls -la "$BUILD_PATH"

if [ -n "$(ls -A "$BUILD_PATH")" ]; then
  echo "directory BUILD_PATH $BUILD_PATH is empty"
fi

exit $UNITY_EXIT_CODE;
