#!/usr/bin/env bash

source ci-setup.sh

echo "Building for $BUILD_TARGET at $PROJECT_PATH in $BUILD_PATH"

mkdir -p "$BUILD_PATH"

./ci-import-required-packages.sh
./ci-setup-license.sh # we need to re-import the license after we import something

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
  -quit \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -logFile "$PROJECT_PATH/build-logs.txt" \
  -buildTarget "$BUILD_TARGET" \
  -customBuildTarget "$BUILD_TARGET" \
  -customBuildName "$BUILD_NAME" \
  -customBuildPath "$BUILD_PATH" \
  -executeMethod BuildCommand.PerformBuild

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

if [ -z "$(ls -A "$BUILD_PATH")" ]; then
  echo "directory BUILD_PATH $BUILD_PATH is empty"
  UNITY_EXIT_CODE=4
fi

exit $UNITY_EXIT_CODE;