#!/usr/bin/env bash
echo Downloading Required Packages

echo "${GPG_PRIVATE_KEY_BASE64}" | base64 -d > private.gpg
gpg  --batch --import private.gpg
curl -L 'https://renderer-artifacts.decentraland.org/artifacts/requiredPackages.unitypackage.gpg' -o requiredPackages.unitypackage.gpg

gpg --output requiredPackages.unitypackage --decrypt requiredPackages.unitypackage.gpg

echo Finished downloading Required Packages

echo Begin importing Required Packages

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
-quit \
-batchmode \
-importPackage $(pwd)/requiredPackages.unitypackage \
-projectPath "$PROJECT_PATH"

echo Ended importing Required Packages