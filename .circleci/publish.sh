#! /bin/bash

export MAJOR=`[ "$CIRCLE_BRANCH" == "release" ] && echo "3" \
    || ([ "$CIRCLE_BRANCH" == "staging" ] && echo "2" \
    || ([ "$CIRCLE_BRANCH" == "master" ] && echo "1" \
    || echo "0"))`;
export MINOR=`cat Assets/Scripts/MainScripts/DCL/Configuration/Configuration.cs | grep 'version = "[0-9.]\+' | grep -o '\.[0-9.]\+'`
export TAG=`[ "$CIRCLE_BRANCH" == "master" ] && echo "" || echo "$CIRCLE_BRANCH" | sed -e 's/[^a-zA-Z0-9-]/-/g'`
export BUILD_VERSIONING=${MAJOR}${MINOR}'.'${CIRCLE_BUILD_NUM}`[ "$TAG" ] && echo "-$TAG"`

echo "Building package.json for $BUILD_VERSIONING"


function publish() {
    # Build package.json
    cd $BUILD_PATH
    echo "Build path is $BUILD_PATH and cwd is $PWD -- Tag is $TAG"
    echo '{"name": "decentraland-renderer", "version": "'${BUILD_VERSIONING}'", "license": "Apache-2.0", "devDependencies": { "npm": "5.6.0" } }' > package.json

    # Export the name of the package as a file
    echo 'module.exports = "'${BUILD_VERSIONING}'";' > index.js

    # Delete unnecessary files from the build
    rm -f game.js index.html Build/UnityLoader.js Build/unity.json
    # Move all `.unityweb` files into the root build folder
    mv Build/* .
    # Publish on npm
    npx npm publish --tag `[ "$CIRCLE_BRANCH" == "master" ] && echo "latest" || echo $TAG`
}

publish

