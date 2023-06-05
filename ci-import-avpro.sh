#!/usr/bin/env bash
echo Downloading AVProVideo

echo "${GPG_PRIVATE_KEY_BASE64}" | base64 -d > private.gpg
gpg  --batch --import private.gpg
curl -L 'https://renderer-artifacts.decentraland.org/artifacts/DCL_AVProVideo_2.8.0_ULTRA.unitypackage.gpg' -o DCL_AVProVideo_2.8.0_ULTRA.unitypackage.gpg

gpg --output DCL_AVProVideo_2.8.0_ULTRA.unitypackage --decrypt DCL_AVProVideo_2.8.0_ULTRA.unitypackage.gpg

echo Finished downloading AVProVideo

echo Begin importing AVProVideo

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
-quit \
-batchmode \
-importPackage $(pwd)/DCL_AVProVideo_2.8.0_ULTRA.unitypackage \
-projectPath "$PROJECT_PATH"

echo Ended importing AvProVideo