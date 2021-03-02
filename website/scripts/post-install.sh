#!/bin/sh

echo "Post Install Script:"
echo "Copy files & dir from decentraland-kernel to public"
cp -r ./node_modules/decentraland-kernel/default-profile ./public;
cp -r ./node_modules/decentraland-kernel/dist/website.js ./public;
cp -r ./node_modules/decentraland-kernel/dist/0.js ./public;
cp -r ./node_modules/decentraland-kernel/dist/1.js ./public;
cp -r ./node_modules/decentraland-kernel/dist/2.js ./public;
cp -r ./node_modules/decentraland-kernel/dist/3.js ./public;
cp -r ./node_modules/decentraland-kernel/loader ./public;
cp -r ./node_modules/decentraland-kernel/systems ./public;
cp -r ./node_modules/decentraland-kernel/unity ./public;
cp -r ./node_modules/decentraland-kernel/voice-chat-codec ./public;

echo "Setting kernel version"

if [ ! -f ".env" ]; then
  echo "file .env does not exist. creating..."
  touch .env
fi

echo "Creating hash versions"
node ./scripts/hash_generator.js

echo ""
echo "Post install script done! 😘😘😘"
echo ""
