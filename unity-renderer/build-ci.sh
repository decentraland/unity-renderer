#!/usr/bin/env bash

set -e
set -x

echo "Building for $BUILD_TARGET"

export BUILD_PATH=./Builds/$BUILD_NAME/
mkdir -p $BUILD_PATH

cd $BUILD_PATH
export BUILD_PATH=$PWD
cd ../..

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd) \
  -quit \
  -batchmode \
  -buildTarget $BUILD_TARGET \
  -customBuildTarget $BUILD_TARGET \
  -customBuildName $BUILD_NAME \
  -customBuildPath $BUILD_PATH \
  -customBuildOptions AcceptExternalModificationsToPlayer \
  -executeMethod BuildCommand.PerformBuild \
  -logFile /tmp/buildlog.txt

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

exit $UNITY_EXIT_CODE;