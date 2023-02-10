#!/usr/bin/env bash
if [ -z "$DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64" ]; then
  echo 'DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64 not present. License won''t be configured'
else
  LICENSE=$(echo "${DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64}" | base64 -di | tr -d '\r')

  echo "Writing LICENSE to license file /root/.local/share/unity3d/Unity/Unity_lic.ulf"
  echo "$LICENSE" > /root/.local/share/unity3d/Unity/Unity_lic.ulf
fi