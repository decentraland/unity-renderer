#!/usr/bin/env bash

echo $BUILD_PATH
ls -lah $BUILD_PATH
pwd
ls -lah .

cd $BUILD_PATH

source ci-setup.sh

set -x

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity } \
        -batchmode \
        -logFile /dev/stdout \
        -projectPath "$PROJECT_PATH" \
        -buildTarget "$BUILD_TARGET" \
        -runTests \
        -testPlatform EditMode \
        -testResults "$PROJECT_PATH/editmode-results.xml" \
        -enableCodeCoverage \
        -coverageResultsPath "$PROJECT_PATH/CodeCoverage" \
        -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateHtmlReportHistory;generateBadgeReport;assemblyFilters:+Assembly-CSharp" \
        -debugCodeOptimization

# Catch exit code
UNITY_EXIT_CODE=$?

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
