#!/bin/bash

######################################################
# This file deploys the ./static folder to the S3 bucket
# and creates an invalidation in the cloudfront caché
######################################################

set -u # no unbound variables

if [[ "$CIRCLE_BRANCH" = gh-readonly-queue/* ]]; then
  echo "Skipping preview publish for branch ${CIRCLE_BRANCH}"
  exit 0
fi

# Upload artifacts
npx @dcl/cdn-uploader@next \
  --bucket "$S3_BUCKET" \
  --local-folder "/tmp/workspace/unity-renderer/browser-interface/static" \
  --bucket-folder "branch/${CIRCLE_BRANCH}" \
  --concurrency 10 \
  --config scripts/cdn-uploader-config.yml


set +u # unbound variables
