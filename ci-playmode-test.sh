#!/usr/bash


set -e
set -x
mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/
set +x

ls -lah /root/.cache/unity3d

if [ -z "$UNITY_LICENSE_CONTENT_BASE64" ]; then
  echo 'UNITY_LICENSE_CONTENT_BASE64 not present. License won''t be configured'
else
  LICENSE=$(echo "${UNITY_LICENSE_CONTENT_BASE64}" | base64 -d | tr -d '\r')

  echo "Writing LICENSE to license file /root/.local/share/unity3d/Unity/Unity_lic.ulf"
  echo "$LICENSE" > /root/.local/share/unity3d/Unity/Unity_lic.ulf

  ${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity } \
    -quit \
    -nographics \
    -logFile /dev/stdout \
    -batchmode \
    -manualLicenseFile /root/.local/share/unity3d/Unity/Unity_lic.ulf
fi

set -x

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity } \
        -batchmode \
        -logFile /dev/stdout \
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
