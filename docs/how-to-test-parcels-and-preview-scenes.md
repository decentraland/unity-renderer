## How to debug with local test parcels and preview scenes

### Test Scenes

You can build the test scenes which are used by entering kernel with the `debug` mode:

    make test-scenes

If you want to run the client in `debug` mode, append the following query parameter to the URL:

    http://localhost:3000/?DEBUG_MODE

To spawn in a specific set of coordinates append the following query paramter:

    http://localhost:3000/?DEBUG_MODE&position=10,10



If you want the test scenes to be updated dynamically after the first build, you should run `make watch-only-test-scenes` in another terminal and keep it watching for changes.

### Creating new test scenes
It is possible to define new scenes inside this repo for testing purposes. To do so, create a new folder in `public/test-scenes`. There are several conventions to be followed regarding the name of these folders and the positions of the parcels, these can be found in the [README](https://github.com/decentraland/client/blob/master/public/test-scenes/README.md) file.

To edit and make sure that `make watch` is rebuilding the scene when you are hacking on a new feature of the kernel, make sure to modify `targets/scenes/basic-scenes.json` and point to the scene you're working on.

### Preview Mode Scenes

#### Unity Editor debugging with dcl scene in "preview mode"

1. Run 'dcl start' to open the scene in preview mode. Leave the server running and close the newly-opened browser tab
2. In Unity Editor, in the "InitialScene" scene, select the WSSController gameobject and untoggle the "Open Browser When Start", and toggle the "Use client debug mode". (Make sure the SceneController has the "Debug Scenes" toggle OFF)
3. In Unity Editor hit PLAY
4. In a new browser tab go to http://localhost:8000/?UNITY_ENABLED=true&DEBUG_MODE&LOCAL_COMMS&position=0%2C-1&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl
5. Go back to unity and the scene should start loading in there almost immediately

#### DCL scene in preview mode using a local Kernel version

1. In the explorer repo directory, run a `make watch` once
2. Kill the server and run `make npm-link`
3. In the scene directory run `npm link decentraland-ecs`
4. In the scene directory run `dcl start` and it should already be using the local version of the client