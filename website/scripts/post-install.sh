#!/bin/sh

echo "Post Install Script:"
echo "Copy files & dir from decentraland-kernel to public"
cp -r ./node_modules/decentraland-kernel/default-profile ./public;
cp -r ./node_modules/decentraland-kernel/dist/website.js ./public;
cp -r ./node_modules/decentraland-kernel/loader ./public;
cp -r ./node_modules/decentraland-kernel/systems ./public;
cp -r ./node_modules/decentraland-kernel/unity ./public;
cp -r ./node_modules/decentraland-kernel/voice-chat-codec ./public;

echo "Setting kernel version"
hash=$(git rev-parse --short HEAD)
if [ ! -f ".env" ]; then
  echo "file .env does not exist. creating..."
  touch .env
fi

sed -i.bak '/REACT_APP_EXPLORER_VERSION/d' .env
rm -fv .env.bak
echo "REACT_APP_EXPLORER_VERSION=${hash}" >>.env

echo ""
echo "Post install script done! 😘😘😘"
echo ""
