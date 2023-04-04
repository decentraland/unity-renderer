#!/usr/bin/env bash

source ci-setup.sh

echo "Running benchmark tests for $PROJECT_PATH"

disable_sentry

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -logFile "$PROJECT_PATH/benchmark-logs.txt" \
  -runTests \
  -testPlatform PlayMode \
  -testCategory Benchmark \
  -testResults "$PROJECT_PATH/benchmark-results.xml"  

# Catch exit code
UNITY_EXIT_CODE=$?

cat "$PROJECT_PATH/benchmark-results.xml"

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
