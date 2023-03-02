#!/usr/bin/env bash
echo Downloading AVProVideo

echo "${GPG_PRIVATE_KEY_BASE64}" | base64 -d > private.gpg
gpg  --batch --import private.gpg
curl -L 'https://renderer-artifacts.decentraland.org/artifacts/Custom_AVProVideo_2.6.7_ULTRA.unitypackage.gpg' -o Custom_AVProVideo_2.6.7_ULTRA.unitypackage.gpg

gpg --output Custom_AVProVideo_2.6.7_ULTRA.unitypackage --decrypt Custom_AVProVideo_2.6.7_ULTRA.unitypackage.gpg

echo Finished downloading AVProVideo

echo Begin importing AVProVideo

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_PATH/Editor/Unity \
-quit \
-batchmode \
-importPackage $(pwd)/Custom_AVProVideo_2.6.7_ULTRA.unitypackage \
-projectPath "$PROJECT_PATH"

echo Ended importing AvProVideo