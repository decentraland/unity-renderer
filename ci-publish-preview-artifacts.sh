#!/bin/bash

######################################################
# This file deploys the ./dist folder to the S3 bucket
# and creates an invalidation in the cloudfront caché
######################################################

set -u # no unbound variables

if [[ "$CIRCLE_BRANCH" = gh-readonly-queue/* ]]; then
  echo "Skipping preview publish for branch ${CIRCLE_BRANCH}"
  exit 0
fi

# Get version
PACKAGE_PATH=/tmp/workspace/unity-renderer/browser-interface/static/package.json
VERSION=$(cat ${PACKAGE_PATH} | jq -r .version)

# Dump version
ARTIFACTS_PATH=/tmp/workspace/unity-renderer/unity-artifacts/
echo "{\"version\":\"${VERSION}\"}" > ${ARTIFACTS_PATH}/version.json

# Upload artifacts for preview
aws s3 sync ${ARTIFACTS_PATH} "s3://${S3_BUCKET}/desktop/${CIRCLE_BRANCH}" --acl public-read

# Invalidate cache
aws configure set preview.cloudfront true
aws configure set preview.create-invalidation true
aws cloudfront create-invalidation --distribution-id "${CLOUDFRONT_DISTRIBUTION}" --paths "/desktop/${CIRCLE_BRANCH}/*"

# Upload artifacts to prod
if [ "${CIRCLE_BRANCH}" == "main" ] || [ "${CIRCLE_BRANCH}" == "dev" ]; then
  aws s3 sync ${ARTIFACTS_PATH} "s3://${S3_BUCKET}/release-desktop/${VERSION}" --acl public-read
fi