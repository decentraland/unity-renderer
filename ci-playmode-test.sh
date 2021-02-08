#!/usr/bin/env bash

set -x

export UNITY_DIR="$(pwd)"

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity } \
        -batchmode \
        -logFile /dev/stdout \
        -projectPath "$UNITY_DIR" \
        -runTests \
        -testPlatform PlayMode \
        -testResults "$UNITY_DIR/playmode-results.xml" \
        -enableCodeCoverage \
        -coverageResultsPath "$UNITY_DIR/CodeCoverage" \
        -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateHtmlReportHistory;generateBadgeReport;assemblyFilters:+Assembly-CSharp" \
        -manualLicenseFile /root/.local/share/unity3d/Unity/Unity_lic.ulf \
        -debugCodeOptimization

# Catch exit code
UNITY_EXIT_CODE=$?

cat "$(pwd)/playmode-results.xml"

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
