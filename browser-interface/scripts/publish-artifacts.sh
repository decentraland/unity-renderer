#!/bin/bash

######################################################
# This file deploys the ./dist folder to the S3 bucket
# and creates an invalidation in the cloudfront caché
######################################################

# Upload artifacts
aws s3 sync /tmp/workspace/unity-renderer/browser-interface/dist "s3://${S3_BUCKET}/branch/${CIRCLE_BRANCH}" --acl public-read

# Invalidate cache
aws configure set preview.cloudfront true
aws configure set preview.create-invalidation true
aws cloudfront create-invalidation --distribution-id "${CLOUDFRONT_DISTRIBUTION}" --paths "branch/${CIRCLE_BRANCH}"