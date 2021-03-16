#!/usr/bin/env bash

source ci-setup.sh

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
        -batchmode \
        -logFile "$PROJECT_PATH/editmode-logs.txt" \
        -runTests \
        -testPlatform EditMode \
        -testResults "$PROJECT_PATH/editmode-results.xml" \
        -enableCodeCoverage \
        -coverageResultsPath "$PROJECT_PATH/CodeCoverage" \
        -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateBadgeReport" \
        -debugCodeOptimization

# Catch exit code
UNITY_EXIT_CODE=$?

mkdir -p test-results/editormode
cp editmode-results.xml test-results/editormode/results.xml || true

# Print unity log output
cat "editmode-results.xml"
cat "editmode-results.xml" | grep test-run | grep Passed

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
