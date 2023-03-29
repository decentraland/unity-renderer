#!/usr/bin/env bash

source ci-setup.sh

echo "Running editmode tests for $PROJECT_PATH"

# Disable Sentry
disable_sentry

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -logFile "$PROJECT_PATH/editmode-logs.txt" \
  -runTests \
  -testPlatform EditMode \
  -testResults "$PROJECT_PATH/editmode-results.xml" \
  -testCategory "EditModeCI"

# Catch exit code
UNITY_EXIT_CODE=$?

# Print unity log output
ls -la $PROJECT_PATH
find /tmp/workspace/unity-renderer -name editmode-results.xml
find $PROJECT_PATH -name editmode-results.xml
cat "$PROJECT_PATH/editmode-results.xml"

# Display results
if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

exit $UNITY_EXIT_CODE
