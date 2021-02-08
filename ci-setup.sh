#!/usr/bin/env bash

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
