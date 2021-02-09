#!/usr/bash

source ci-setup.sh

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity } \
        -batchmode \
        -logFile "$PROJECT_PATH/playmode-logs.txt" \
        -projectPath "$PROJECT_PATH" \
        -runTests \
        -testPlatform PlayMode \
        -testResults "$PROJECT_PATH/playmode-results.xml" \
        -enableCodeCoverage \
        -coverageResultsPath "$PROJECT_PATH/CodeCoverage" \
        -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateHtmlReportHistory;generateBadgeReport;assemblyFilters:+Assembly-CSharp" \
        -debugCodeOptimization

# Catch exit code
UNITY_EXIT_CODE=$?

cat "$PROJECT_PATH/playmode-logs.txt"
cat "$PROJECT_PATH/playmode-results.xml"

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
