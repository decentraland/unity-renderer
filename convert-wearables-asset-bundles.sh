#!/usr/bin/env bash
# THIS FILE IS USED BY ENTRYPOINT OF THE WEARABLE COLLECTIONS ASSET BUNDLE CONVERSOR

set -u # fail if any env var is not set

source ci-setup.sh

mkdir -p "$OUTPUT_DIR"

echo "Running AB conversor for wearables collection $WEARABLES_COLLECTION_URN_ID at $CONTENT_URL > $OUTPUT_DIR"
echo "Project path: $PROJECT_PATH"

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' "$UNITY_PATH/Editor/Unity" \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -batchmode \
  -executeMethod DCL.ABConverter.WearablesCollectionClient.ExportWearablesCollectionToAssetBundles \
  -wearablesCollectionUrnId "$WEARABLES_COLLECTION_URN_ID" \
  -logFile "$LOCAL_LOG_FILE" \
  -baseUrl "$CONTENT_URL" \
  -output "$OUTPUT_DIR"

UNITY_EXIT_CODE=$?

exit $UNITY_EXIT_CODE